using System;

namespace DevToolVault.Models
{
    public class FileStatistics
    {
        public long TotalFiles { get; set; }
        public long TotalFolders { get; set; }
        public long TotalSize { get; set; }

        public void Reset()
        {
            TotalFiles = 0;
            TotalFolders = 0;
            TotalSize = 0;
        }

        public string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            int order = 0;
            double len = bytes;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        public override string ToString()
        {
            return $"Pastas: {TotalFolders}, Arquivos: {TotalFiles}, Tamanho total: {FormatFileSize(TotalSize)}";
        }
    }
}