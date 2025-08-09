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

            var rootItem = new FileSystemItem
            {
                FullName = path,
                Name = Path.GetFileName(path),
                IsDirectory = true,
                IsChecked = true,
                IsThreeState = false
            };

            PopulateDirectory(rootItem, filterManager.GetActiveProfile());
            treeFiles.Items.Add(rootItem);
        }

        private void PopulateDirectory(FileSystemItem parentItem, FilterProfile profile)
        {
            try
            {
                var directoryInfo = new DirectoryInfo(parentItem.FullName);

                // Obtém as opções do perfil
                var options = TreeOptions.FromFilterProfile(profile);
                var fileFilter = new FileFilter(options);

                // Processa subdiretórios
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
                            IsThreeState = false,
                            Parent = parentItem
                        };

                        parentItem.Children.Add(dirItem);
                        PopulateDirectory(dirItem, profile);
                    }
                }

                // Processa arquivos
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
                            IsThreeState = false,
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
                if (!item.IsDirectory) // Só adiciona arquivos diretamente
                {
                    selectedItems.Add(item);
                }
                else
                {
                    // Para pastas, adiciona todos os arquivos selecionados dentro dela
                    foreach (var child in item.Children)
                    {
                        GetSelectedItemsRecursive(child, selectedItems);
                    }
                }
            }
            else if (item.IsChecked == null) // Estado indeterminado
            {
                // Adiciona apenas os itens filhos que estão selecionados
                foreach (var child in item.Children)
                {
                    GetSelectedItemsRecursive(child, selectedItems);
                }
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            var item = checkBox.DataContext as FileSystemItem;

            if (item != null)
            {
                UpdateChildSelection(item, true);
                UpdateParentSelection(item);
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            var item = checkBox.DataContext as FileSystemItem;

            if (item != null)
            {
                UpdateChildSelection(item, false);
                UpdateParentSelection(item);
            }
        }

        private void UpdateChildSelection(FileSystemItem item, bool isChecked)
        {
            item.IsChecked = isChecked;
            item.IsThreeState = false;

            foreach (var child in item.Children)
            {
                UpdateChildSelection(child, isChecked);
            }
        }

        private void UpdateParentSelection(FileSystemItem item)
        {
            if (item.Parent == null) return;

            var parent = item.Parent;
            var checkedChildren = parent.Children.Count(c => c.IsChecked == true);
            var uncheckedChildren = parent.Children.Count(c => c.IsChecked == false);

            if (checkedChildren == parent.Children.Count)
            {
                parent.IsChecked = true;
                parent.IsThreeState = false;
            }
            else if (uncheckedChildren == parent.Children.Count)
            {
                parent.IsChecked = false;
                parent.IsThreeState = false;
            }
            else
            {
                parent.IsChecked = false;
                parent.IsThreeState = true;
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
            ExpandAllItems(treeFiles.Items);
        }

        private void BtnCollapseAll_Click(object sender, RoutedEventArgs e)
        {
            CollapseAllItems(treeFiles.Items);
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(RootPath) && FilterManager != null)
            {
                LoadDirectory(RootPath, FilterManager);
            }
        }

        private void ExpandAllItems(ItemCollection items)
        {
            foreach (var item in items)
            {
                if (item is TreeViewItem treeViewItem)
                {
                    treeViewItem.IsExpanded = true;
                    ExpandAllItems(treeViewItem.Items);
                }
            }
        }

        private void CollapseAllItems(ItemCollection items)
        {
            foreach (var item in items)
            {
                if (item is TreeViewItem treeViewItem)
                {
                    treeViewItem.IsExpanded = false;
                    CollapseAllItems(treeViewItem.Items);
                }
            }
        }
    }
}