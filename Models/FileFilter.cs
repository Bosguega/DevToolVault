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

            // Verifica padrões de ignorar
            if (_options.IgnorePatterns != null)
            {
                foreach (var pattern in _options.IgnorePatterns)
                {
                    if (pattern.StartsWith("*."))
                    {
                        string extension = pattern.Substring(1);
                        if (name.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                            return true;
                    }
                    else if (pattern.Contains("*"))
                    {
                        string regexPattern = "^" + Regex.Escape(pattern).Replace("\\*", ".*") + "$";
                        if (Regex.IsMatch(name, regexPattern, RegexOptions.IgnoreCase))
                            return true;
                    }
                    else if (string.Equals(name, pattern, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            // Verifica arquivos de sistema se não estiver mostrando
            if (!_options.ShowSystemFiles)
            {
                try
                {
                    FileAttributes attr = File.GetAttributes(path);
                    if ((attr & FileAttributes.Hidden) != 0 || (attr & FileAttributes.System) != 0)
                        return true;
                }
                catch
                {
                    return true;
                }
            }

            return false;
        }
    }
}