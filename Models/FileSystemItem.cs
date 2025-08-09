using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace DevToolVault.Models
{
    public class FileSystemItem : INotifyPropertyChanged
    {
        private bool? _isChecked = true; // true, false, null (parcial)
        private bool _isExpanded;

        public string FullName { get; set; }
        public string Name { get; set; }
        public bool IsDirectory { get; set; }
        public FileSystemItem Parent { get; set; }

        public ObservableCollection<FileSystemItem> Children { get; } = new();

        public bool? IsChecked
        {
            get => _isChecked;
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged();
                }
            }
        }

        public string RelativePath
        {
            get
            {
                if (Parent == null) return Name ?? string.Empty;
                var parentPath = Parent.RelativePath;
                return string.IsNullOrEmpty(parentPath) ? Name : Path.Combine(parentPath, Name);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}