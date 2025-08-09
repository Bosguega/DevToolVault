using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

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
            string extension = Path.GetExtension(path).ToLowerInvariant();

            if (isDirectory)
            {
                foreach (var pattern in _options.IgnorePatterns)
                {
                    if (MatchesPattern(name, path, pattern))
                        return true;
                }
            }
            else
            {
                if (_options.ShowOnlyCodeFiles && _options.CodeExtensions != null)
                {
                    if (!_options.CodeExtensions.Any(ext =>
                        string.Equals(ext, extension, StringComparison.OrdinalIgnoreCase)))
                    {
                        return true;
                    }
                }

                foreach (var pattern in _options.IgnorePatterns)
                {
                    if (MatchesPattern(name, path, pattern))
                        return true;
                }
            }

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
                    return true;
                }
            }

            return false;
        }

        public bool ShouldIgnoreDebug(string path, bool isDirectory)
        {
            string name = Path.GetFileName(path);
            string extension = Path.GetExtension(path).ToLowerInvariant();

            if (isDirectory)
            {
                foreach (var pattern in _options.IgnorePatterns)
                {
                    if (MatchesPattern(name, path, pattern))
                    {
                        Console.WriteLine($"Ignorando diretório: {path} (padrão: {pattern})");
                        return true;
                    }
                }
            }
            else
            {
                if (_options.ShowOnlyCodeFiles && _options.CodeExtensions != null)
                {
                    if (!_options.CodeExtensions.Any(ext =>
                        string.Equals(ext, extension, StringComparison.OrdinalIgnoreCase)))
                    {
                        Console.WriteLine($"Ignorando arquivo (extensão não permitida): {path}");
                        return true;
                    }
                }

                foreach (var pattern in _options.IgnorePatterns)
                {
                    if (MatchesPattern(name, path, pattern))
                    {
                        Console.WriteLine($"Ignorando arquivo: {path} (padrão: {pattern})");
                        return true;
                    }
                }
            }

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

        private bool MatchesPattern(string name, string fullPath, string pattern)
        {
            string normPath = fullPath.Replace('\\', '/').ToLowerInvariant();
            string normName = name.ToLowerInvariant();
            string normPattern = pattern.Replace('\\', '/').ToLowerInvariant();

            if (normPattern.StartsWith("*."))
            {
                string ext = normPattern.Substring(1); // inclui o ponto
                return normName.EndsWith(ext, StringComparison.OrdinalIgnoreCase);
            }
            else if (normPattern.Contains("*") || normPattern.Contains("?"))
            {
                bool patternHasSlash = normPattern.Contains('/');
                string target = patternHasSlash ? normPath : normName;
                string regexPattern = "^" + Regex.Escape(normPattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";
                return Regex.IsMatch(target, regexPattern, RegexOptions.IgnoreCase);
            }
            else
            {
                bool patternHasSlash = normPattern.Contains('/');
                if (patternHasSlash)
                {
                    return normPath.Contains(normPattern);
                }
                else
                {
                    return string.Equals(normName, normPattern, StringComparison.OrdinalIgnoreCase);
                }
            }
        }

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