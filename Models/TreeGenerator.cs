using System.IO;
using System.Text;

namespace DevToolVault.Models
{
    public class TreeGenerator
    {
        private readonly FileFilter _fileFilter;
        private readonly FileStatistics _statistics;

        public TreeGenerator(FileFilter fileFilter, FileStatistics statistics)
        {
            _fileFilter = fileFilter;
            _statistics = statistics;
        }

        public string GenerateTree(string rootPath, TreeOptions options)
        {
            var builder = new StringBuilder();
            var rootDir = new DirectoryInfo(rootPath);

            builder.AppendLine(rootDir.FullName);
            AppendDirectoryContents(rootDir, builder, "", options);

            return builder.ToString();
        }

        private void AppendDirectoryContents(DirectoryInfo dir, StringBuilder sb, string indent, TreeOptions options)
        {
            // Verifica se o diretório deve ser ignorado
            if (_fileFilter.ShouldIgnoreDebug(dir.FullName, true))
                return;

            _statistics.TotalFolders++;
            try
            {
                FileInfo[] files;
                DirectoryInfo[] subDirs;
                try
                {
                    files = dir.GetFiles();
                    subDirs = dir.GetDirectories();
                }
                catch (UnauthorizedAccessException)
                {
                    sb.AppendLine($"{indent}└── [Acesso negado]");
                    return;
                }

                // Filtra os arquivos e diretórios usando o FileFilter
                var visibleFiles = files.Where(f => !_fileFilter.ShouldIgnoreDebug(f.FullName, false)).ToArray();
                var visibleSubDirs = subDirs.Where(d => !_fileFilter.ShouldIgnoreDebug(d.FullName, true)).ToArray();

                int totalItems = visibleFiles.Length + visibleSubDirs.Length;
                if (totalItems == 0 && options.IgnoreEmptyFolders)
                    return;

                for (int i = 0; i < visibleFiles.Length; i++)
                {
                    var file = visibleFiles[i];
                    bool isLast = (i == visibleFiles.Length - 1) && (visibleSubDirs.Length == 0);
                    string symbol = isLast ? "└──" : "├──";
                    string size = options.ShowFileSize ? $" ({_statistics.FormatFileSize(file.Length)})" : "";
                    sb.AppendLine($"{indent}{symbol} {file.Name}{size}");
                    _statistics.TotalFiles++;
                    _statistics.TotalSize += file.Length;
                }

                for (int i = 0; i < visibleSubDirs.Length; i++)
                {
                    var subDir = visibleSubDirs[i];
                    bool isLast = (i == visibleSubDirs.Length - 1) && (visibleFiles.Length == 0);
                    string symbol = isLast ? "└──" : "├──";
                    sb.AppendLine($"{indent}{symbol} {subDir.Name}/");
                    string newIndent = indent + (isLast ? "    " : "│   ");
                    AppendDirectoryContents(subDir, sb, newIndent, options);
                }

                if (totalItems == 0)
                {
                    sb.AppendLine($"{indent}└── [vazio]");
                }
            }
            catch
            {
                sb.AppendLine($"{indent}└── [Erro ao ler]");
            }
        }
    }
}
