using System;
using System.Collections.Generic;
using System.IO;

namespace DevToolVault.Models
{
    public class FileSystemItem
    {
        public string FullName { get; set; }
        public string Name { get; set; }
        public bool IsDirectory { get; set; }
        public bool IsChecked { get; set; }
        public bool? IsThreeState { get; set; } // Para pastas com seleção parcial
        public List<FileSystemItem> Children { get; set; } = new List<FileSystemItem>();
        public FileSystemItem Parent { get; set; }

        public string RelativePath
        {
            get
            {
                if (Parent == null)
                    return "";

                var parentPath = Parent.RelativePath;
                return string.IsNullOrEmpty(parentPath)
                    ? Name
                    : Path.Combine(parentPath, Name);
            }
        }
    }
}