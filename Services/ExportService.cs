// DevToolVault/Services/ExportService.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevToolVault.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace DevToolVault.Services
{
    /// <summary>
    /// Serviço responsável por exportar arquivos selecionados em diferentes formatos: TXT, Markdown, PDF e ZIP.
    /// Exporta o conteúdo bruto dos arquivos, sem destaque de sintaxe, para manter fidelidade máxima.
    /// Ideal para uso com IAs e análise de código.
    /// </summary>
    public class ExportService : IExportService
    {
        public enum ExportFormat
        {
            Text,
            Markdown,
            Pdf,
            Zip
        }

        /// <summary>
        /// Exporta os arquivos selecionados no formato especificado.
        /// </summary>
        public async Task ExportAsync(List<FileSystemItem> files, string outputPath, ExportFormat format)
        {
            switch (format)
            {
                case ExportFormat.Text:
                    await ExportTextOrMarkdownAsync(files, outputPath, false);
                    break;
                case ExportFormat.Markdown:
                    await ExportTextOrMarkdownAsync(files, outputPath, true);
                    break;
                case ExportFormat.Pdf:
                    await ExportPdfRawAsync(files, outputPath);
                    break;
                case ExportFormat.Zip:
                    await ExportZipAsync(files, outputPath);
                    break;
                default:
                    throw new ArgumentException("Formato de exportação não suportado.");
            }
        }

        #region Exportação Texto e Markdown (sem destaque)

        private async Task ExportTextOrMarkdownAsync(List<FileSystemItem> files, string outputPath, bool isMarkdown)
        {
            var fileTasks = files.Select(ReadFileSafeAsync);
            var results = await Task.WhenAll(fileTasks);

            var sb = new StringBuilder();

            // Forçar UTF-8 com BOM
            var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);

            if (isMarkdown)
            {
                sb.AppendLine("# Código Exportado\n");
                sb.AppendLine($"*Exportado em: {DateTime.Now:yyyy-MM-dd HH:mm}*\n");
            }

            foreach (var (item, content) in results)
            {
                var relativePath = item.RelativePath.Replace('\\', '/');

                if (isMarkdown)
                {
                    sb.AppendLine($"<!-- Arquivo: {relativePath} -->");
                    sb.AppendLine("```");
                    sb.Append(content);
                    sb.AppendLine("```");
                }
                else
                {
                    sb.AppendLine($"// Arquivo: {relativePath}");
                    sb.Append(content);
                }

                // Separador claro
                sb.AppendLine("\n" + new string('-', 80) + "\n");
            }

            await File.WriteAllTextAsync(outputPath, sb.ToString(), encoding);
        }

        private static async Task<(FileSystemItem, string)> ReadFileSafeAsync(FileSystemItem item)
        {
            try
            {
                var content = await File.ReadAllTextAsync(item.FullName, Encoding.UTF8);
                return (item, content);
            }
            catch (Exception ex)
            {
                return (item, $"// Erro ao ler {item.RelativePath}: {ex.Message}\n");
            }
        }

        #endregion

        #region Exportação PDF (sem destaque de sintaxe, apenas conteúdo bruto)

        private async Task ExportPdfRawAsync(List<FileSystemItem> files, string outputPath)
        {
            using var document = new Document();
            using var writer = PdfWriter.GetInstance(document, new FileStream(outputPath, FileMode.Create));
            document.Open();

            // Fonte monoespaçada para preservar indentação
            var font = FontFactory.GetFont("Courier", 8, Font.NORMAL);
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
            var pathFont = FontFactory.GetFont(FontFactory.HELVETICA, 10, Font.ITALIC);

            // Título
            document.Add(new Paragraph("Código Exportado", titleFont));
            document.Add(new Paragraph($"Data: {DateTime.Now:yyyy-MM-dd HH:mm}\n", pathFont));

            foreach (var item in files)
            {
                try
                {
                    var content = await ReadFileContentAsync(item.FullName);
                    var relativePath = item.RelativePath.Replace('\\', '/');

                    // Cabeçalho do arquivo
                    document.Add(new Paragraph($"Arquivo: {relativePath}", pathFont));

                    // Usar Preformatted para manter espaços e quebras exatamente como estão
                    var preformatted = new Paragraph(content, font);
                    preformatted.IndentationLeft = 5f;
                    preformatted.SpacingAfter = 10f;
                    document.Add(preformatted);

                    // Separador
                    document.Add(new Paragraph("\n" + new string('-', 80) + "\n", font));
                }
                catch (Exception ex)
                {
                    var errorFont = new Font(font.BaseFont, 8, Font.NORMAL, BaseColor.Red);
                    document.Add(new Paragraph($"Erro ao ler {item.RelativePath}: {ex.Message}", errorFont));
                }
            }

            document.Close();
        }

        private async Task<string> ReadFileContentAsync(string filePath)
        {
            using var reader = new StreamReader(filePath, Encoding.UTF8);
            return await reader.ReadToEndAsync();
        }

        #endregion

        #region Exportação ZIP

        private async Task ExportZipAsync(List<FileSystemItem> files, string outputPath)
        {
            await Task.Run(() =>
            {
                var tempDir = Path.Combine(Path.GetTempPath(), "DevToolVault_Temp_" + Guid.NewGuid());
                Directory.CreateDirectory(tempDir);

                try
                {
                    var uniqueFiles = EnsureUniqueFilenames(files);
                    var failedFiles = new List<string>();

                    Parallel.ForEach(uniqueFiles, item =>
                    {
                        try
                        {
                            var relativePath = SanitizeRelativePath(item.RelativePath);
                            if (string.IsNullOrEmpty(relativePath) || relativePath.Contains(".."))
                                return;

                            var targetDir = Path.Combine(tempDir, relativePath);
                            var targetDirSafe = Path.GetFullPath(targetDir);

                            if (!targetDirSafe.StartsWith(tempDir + Path.DirectorySeparatorChar))
                                return;

                            Directory.CreateDirectory(Path.GetDirectoryName(targetDirSafe)!);
                            File.Copy(item.FullName, Path.Combine(targetDirSafe, item.Name), true);
                        }
                        catch (Exception ex)
                        {
                            lock (failedFiles)
                            {
                                failedFiles.Add($"{item.RelativePath} ({ex.Message})");
                            }
                        }
                    });

                    if (File.Exists(outputPath))
                        File.Delete(outputPath);

                    ZipFile.CreateFromDirectory(tempDir, outputPath);

                    if (failedFiles.Count > 0)
                    {
                        var errorList = string.Join("\n", failedFiles);
                        Console.WriteLine($"Alguns arquivos falharam ao copiar:\n{errorList}");
                    }
                }
                finally
                {
                    if (Directory.Exists(tempDir))
                        Directory.Delete(tempDir, true);
                }
            });
        }

        private string SanitizeRelativePath(string path)
        {
            path = path.Replace('/', Path.DirectorySeparatorChar)
                       .Replace('\\', Path.DirectorySeparatorChar);
            var parts = path.Split(new[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
            var clean = new List<string>();
            foreach (var part in parts)
            {
                if (part == "..") continue;
                if (part != ".") clean.Add(part);
            }
            return string.Join(Path.DirectorySeparatorChar.ToString(), clean);
        }

        private List<FileSystemItem> EnsureUniqueFilenames(List<FileSystemItem> files)
        {
            var seen = new HashSet<string>();
            var result = new List<FileSystemItem>();
            foreach (var file in files)
            {
                var key = $"{file.RelativePath}_{file.Name}";
                if (seen.Add(key))
                {
                    result.Add(file);
                }
            }
            return result;
        }

        #endregion
    }

    public interface IExportService
    {
        Task ExportAsync(List<FileSystemItem> files, string outputPath, ExportService.ExportFormat format);
    }
}