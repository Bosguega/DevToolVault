using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics; // Adicione esta linha

namespace DevToolVault.Models
{
    public class FileFilter
    {
        private readonly TreeOptions _options;

        public FileFilter(TreeOptions options)
        {
            _options = options;
        }

        public bool ShouldIgnore(string path, bool isDirectory)
        {
            string name = Path.GetFileName(path);
            string extension = Path.GetExtension(path).ToLowerInvariant(); // sempre minúsculo

            // Se for diretório, verifica padrões de ignorar
            if (isDirectory)
            {
                foreach (var pattern in _options.IgnorePatterns)
                {
                    if (MatchesPattern(name, pattern))
                        return true;
                }
            }
            // Se for arquivo
            else
            {
                // Verifica se deve mostrar apenas arquivos de código
                if (_options.ShowOnlyCodeFiles && _options.CodeExtensions != null)
                {
                    // Usa StringComparison.OrdinalIgnoreCase para evitar problemas de case
                    if (!_options.CodeExtensions.Any(ext =>
                        string.Equals(ext, extension, StringComparison.OrdinalIgnoreCase)))
                    {
                        return true; // ignora arquivos que não são de código
                    }
                }

                // Verifica se o nome do arquivo casa com algum padrão de ignorar (ex: *.log, temp.*, etc)
                foreach (var pattern in _options.IgnorePatterns)
                {
                    if (MatchesPattern(name, pattern))
                        return true;
                }
            }

            // Se não deve mostrar arquivos de sistema, verifica atributos
            if (!_options.ShowSystemFiles)
            {
                try
                {
                    FileAttributes attr = File.GetAttributes(path);

                    if ((attr & FileAttributes.Hidden) != 0 ||
                        (attr & FileAttributes.System) != 0 ||
                        (attr & FileAttributes.Temporary) != 0 ||
                        (attr & FileAttributes.Offline) != 0)
                    {
                        return true;
                    }
                }
                catch
                {
                    // Em caso de erro ao ler atributos (acesso negado, etc), ignora o arquivo
                    return true;
                }
            }

            return false;
        }

        public bool ShouldIgnoreDebug(string path, bool isDirectory)
        {
            string name = Path.GetFileName(path);
            string extension = Path.GetExtension(path).ToLowerInvariant();

            // Se for um diretório, verifica se está na lista de ignorados
            if (isDirectory)
            {
                foreach (var pattern in _options.IgnorePatterns)
                {
                    if (MatchesPattern(name, pattern))
                    {
                        Console.WriteLine($"Ignorando diretório: {path} (padrão: {pattern})");
                        return true;
                    }
                }
            }
            // Se for um arquivo
            else
            {
                // Se estiver mostrando apenas arquivos de código, verifica a extensão
                if (_options.ShowOnlyCodeFiles && _options.CodeExtensions != null)
                {
                    if (!_options.CodeExtensions.Contains(extension))
                    {
                        Console.WriteLine($"Ignorando arquivo (extensão não permitida): {path}");
                        return true;
                    }
                }

                // Verifica se o arquivo deve ser ignorado por outros motivos
                foreach (var pattern in _options.IgnorePatterns)
                {
                    if (MatchesPattern(name, pattern))
                    {
                        Console.WriteLine($"Ignorando arquivo: {path} (padrão: {pattern})");
                        return true;
                    }
                }
            }

            // Se não estiver mostrando arquivos de sistema, verifica atributos
            if (!_options.ShowSystemFiles)
            {
                try
                {
                    FileAttributes attr = File.GetAttributes(path);

                    if ((attr & FileAttributes.Hidden) != 0 ||
                        (attr & FileAttributes.System) != 0 ||
                        (attr & FileAttributes.Temporary) != 0 ||
                        (attr & FileAttributes.Offline) != 0)
                    {
                        Console.WriteLine($"Ignorando (arquivo de sistema): {path}");
                        return true;
                    }
                }
                catch
                {
                    Console.WriteLine($"Ignorando (erro ao acessar atributos): {path}");
                    return true;
                }
            }

            Console.WriteLine($"Mostrando: {path}");
            return false;
        }

        private bool MatchesPattern(string name, string pattern)
        {
            // Se o padrão começa com *., é uma extensão
            if (pattern.StartsWith("*."))
            {
                string ext = pattern.Substring(1).ToLowerInvariant();
                return name.EndsWith(ext, StringComparison.OrdinalIgnoreCase);
            }
            // Se o padrão tem curinga, usa regex
            else if (pattern.Contains("*") || pattern.Contains("?"))
            {
                string regexPattern = "^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".");
                return Regex.IsMatch(name, regexPattern, RegexOptions.IgnoreCase);
            }
            // Se o padrão não tem curinga, compara direto
            else
            {
                return string.Equals(name, pattern, StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Verifica se um caminho é de um arquivo de código com base na extensão.
        /// </summary>
        public bool IsCodeFile(string path)
        {
            if (_options.CodeExtensions == null || string.IsNullOrEmpty(path))
                return false;

            string extension = Path.GetExtension(path).ToLowerInvariant();
            return _options.CodeExtensions.Any(ext =>
                string.Equals(ext, extension, StringComparison.OrdinalIgnoreCase));
        }
    }
}