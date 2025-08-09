using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DevToolVault.Models;
using DevToolVault.Filters;

namespace DevToolVault.Controls
{
    public partial class FileSystemTreeView : UserControl
    {
        public FileSystemTreeView()
        {
            InitializeComponent();
        }

        public FileFilterManager FilterManager { get; set; }
        public string RootPath { get; private set; }

        public void LoadDirectory(string path, FileFilterManager filterManager)
        {
            RootPath = path;
            FilterManager = filterManager;

            treeFiles.Items.Clear();

            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
                return;

            var rootName = Path.GetFileName(path);
            if (string.IsNullOrEmpty(rootName))
                rootName = path.TrimEnd(Path.DirectorySeparatorChar);

            var rootItem = new FileSystemItem
            {
                FullName = path,
                Name = rootName,
                IsDirectory = true,
                IsChecked = true
            };

            PopulateDirectory(rootItem, filterManager.GetActiveProfile());
            treeFiles.Items.Add(rootItem);
        }

        private void PopulateDirectory(FileSystemItem parentItem, FilterProfile profile)
        {
            try
            {
                var directoryInfo = new DirectoryInfo(parentItem.FullName);

                var options = TreeOptions.FromFilterProfile(profile);
                var fileFilter = new FileFilter(options);

                foreach (var dir in directoryInfo.GetDirectories())
                {
                    if (!fileFilter.ShouldIgnore(dir.FullName, true))
                    {
                        var dirItem = new FileSystemItem
                        {
                            FullName = dir.FullName,
                            Name = dir.Name,
                            IsDirectory = true,
                            IsChecked = true,
                            Parent = parentItem
                        };

                        parentItem.Children.Add(dirItem);
                        PopulateDirectory(dirItem, profile);
                    }
                }

                foreach (var file in directoryInfo.GetFiles())
                {
                    if (!fileFilter.ShouldIgnore(file.FullName, false))
                    {
                        var fileItem = new FileSystemItem
                        {
                            FullName = file.FullName,
                            Name = file.Name,
                            IsDirectory = false,
                            IsChecked = true,
                            Parent = parentItem
                        };

                        parentItem.Children.Add(fileItem);
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Ignora diretórios sem acesso
            }
        }

        public List<FileSystemItem> GetSelectedItems()
        {
            var selectedItems = new List<FileSystemItem>();

            foreach (FileSystemItem item in treeFiles.Items)
            {
                GetSelectedItemsRecursive(item, selectedItems);
            }

            return selectedItems;
        }

        private void GetSelectedItemsRecursive(FileSystemItem item, List<FileSystemItem> selectedItems)
        {
            if (item.IsChecked == true)
            {
                if (!item.IsDirectory)
                {
                    selectedItems.Add(item);
                }
                else
                {
                    foreach (var child in item.Children)
                    {
                        GetSelectedItemsRecursive(child, selectedItems);
                    }
                }
            }
            else if (item.IsChecked == null) // Estado indeterminado
            {
                foreach (var child in item.Children)
                {
                    GetSelectedItemsRecursive(child, selectedItems);
                }
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e) => OnCheckBoxStateChanged(sender, true);
        private void CheckBox_Unchecked(object sender, RoutedEventArgs e) => OnCheckBoxStateChanged(sender, false);
        private void CheckBox_Indeterminate(object sender, RoutedEventArgs e) => OnCheckBoxStateChanged(sender, null);

        private void OnCheckBoxStateChanged(object sender, bool? state)
        {
            if (sender is CheckBox checkBox && checkBox.DataContext is FileSystemItem item)
            {
                UpdateChildSelection(item, state);
                UpdateParentSelection(item);
            }
        }

        private void UpdateChildSelection(FileSystemItem item, bool? state)
        {
            item.IsChecked = state;

            foreach (var child in item.Children)
            {
                UpdateChildSelection(child, state);
            }
        }

        private void UpdateParentSelection(FileSystemItem item)
        {
            if (item.Parent == null) return;

            var parent = item.Parent;
            int total = parent.Children.Count;
            int checkedChildren = parent.Children.Count(c => c.IsChecked == true);
            int uncheckedChildren = parent.Children.Count(c => c.IsChecked == false);

            if (checkedChildren == total)
            {
                parent.IsChecked = true;
            }
            else if (uncheckedChildren == total)
            {
                parent.IsChecked = false;
            }
            else
            {
                parent.IsChecked = null;
            }

            UpdateParentSelection(parent);
        }

        private void ChkSelectAll_Checked(object sender, RoutedEventArgs e)
        {
            if (chkSelectAll.IsChecked == true)
            {
                foreach (FileSystemItem item in treeFiles.Items)
                {
                    UpdateChildSelection(item, true);
                }
            }
        }

        private void ChkSelectAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (chkSelectAll.IsChecked == false)
            {
                foreach (FileSystemItem item in treeFiles.Items)
                {
                    UpdateChildSelection(item, false);
                }
            }
        }

        private void BtnExpandAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (FileSystemItem item in treeFiles.Items)
            {
                SetExpandedRecursive(item, true);
            }
        }

        private void BtnCollapseAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (FileSystemItem item in treeFiles.Items)
            {
                SetExpandedRecursive(item, false);
            }
        }

        private void SetExpandedRecursive(FileSystemItem item, bool expanded)
        {
            item.IsExpanded = expanded;
            foreach (var child in item.Children)
            {
                SetExpandedRecursive(child, expanded);
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(RootPath) && FilterManager != null)
            {
                LoadDirectory(RootPath, FilterManager);
            }
        }
    }
}