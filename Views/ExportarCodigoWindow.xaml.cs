using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using DevToolVault.Controls;
using DevToolVault.Filters;
using DevToolVault.Models;
using DevToolVault.Services;
using DevToolVault.Utils;
using Ookii.Dialogs.Wpf;

namespace DevToolVault.Views
{
    public partial class ExportarCodigoWindow : Window
    {
        private bool isProcessing = false;
        private readonly FileFilterManager _filterManager;
        private string _projectRoot;
        private readonly ExportService _exportService = new ExportService();

        public ExportarCodigoWindow(FileFilterManager filterManager = null)
        {
            InitializeComponent();

            _filterManager = filterManager ?? new FileFilterManager();

            var activeProfile = _filterManager.GetActiveProfile();
            if (activeProfile == null)
            {
                var selectorWindow = new ProjectTypeSelectorWindow(_filterManager);
                selectorWindow.Owner = this;

                if (selectorWindow.ShowDialog() == true)
                {
                    activeProfile = selectorWindow.SelectedProfile;
                    _filterManager.SetActiveProfile(activeProfile);
                }
                else
                {
                    // Fallback: tenta Flutter ou primeiro disponível
                    activeProfile = _filterManager.GetProfiles().FirstOrDefault(p => p.Name == "Flutter") ??
                                    _filterManager.GetProfiles().FirstOrDefault();

                    if (activeProfile != null)
                        _filterManager.SetActiveProfile(activeProfile);
                }
            }

            SetupUIFromProfile(activeProfile);
            txtCurrentProfile.Text = $"Perfil atual: {activeProfile?.Name}";
        }

        private void SetupUIFromProfile(FilterProfile profile)
        {
            txtCurrentProfile.Text = $"Perfil atual: {profile?.Name}";
        }

        private void BtnSelectProjectType_Click(object sender, RoutedEventArgs e)
        {
            var selectorWindow = new ProjectTypeSelectorWindow(_filterManager);
            selectorWindow.Owner = this;

            if (selectorWindow.ShowDialog() == true)
            {
                var selectedProfile = selectorWindow.SelectedProfile;
                if (selectedProfile != null)
                {
                    _filterManager.SetActiveProfile(selectedProfile);
                    SetupUIFromProfile(selectedProfile);
                    txtCurrentProfile.Text = $"Perfil atual: {selectedProfile.Name}";

                    if (!string.IsNullOrWhiteSpace(txtFolderPath.Text))
                    {
                        fileTreeView.LoadDirectory(txtFolderPath.Text, _filterManager);
                    }
                }
            }
        }

        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog();
            if (dialog.ShowDialog(this) == true)
            {
                txtFolderPath.Text = dialog.SelectedPath;
                _projectRoot = txtFolderPath.Text;
                fileTreeView.LoadDirectory(txtFolderPath.Text, _filterManager);
            }
        }

        private async void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            await ExportSelectedFilesAsync();
        }

        private async Task ExportSelectedFilesAsync()
        {
            var selectedItems = fileTreeView.GetSelectedItems();
            if (!selectedItems.Any())
            {
                MessageBox.Show("Selecione pelo menos um arquivo para exportar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Mapear ComboBox para formato
            var format = cmbExportFormat.SelectedIndex switch
            {
                1 => ExportService.ExportFormat.Markdown,
                2 => ExportService.ExportFormat.Pdf,
                3 => ExportService.ExportFormat.Zip,
                _ => ExportService.ExportFormat.Text
            };

            var filter = format switch
            {
                ExportService.ExportFormat.Markdown => "Arquivo Markdown (*.md)|*.md",
                ExportService.ExportFormat.Pdf => "PDF (*.pdf)|*.pdf",
                ExportService.ExportFormat.Zip => "Arquivo ZIP (*.zip)|*.zip",
                _ => "Arquivo de Texto (*.txt)|*.txt"
            };

            var defaultName = format switch
            {
                ExportService.ExportFormat.Markdown => "codigo.md",
                ExportService.ExportFormat.Pdf => "codigo.pdf",
                ExportService.ExportFormat.Zip => "codigo.zip",
                _ => "codigo.txt"
            };

            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                Filter = filter,
                FileName = defaultName
            };

            if (dlg.ShowDialog(this) != true) return;

            isProcessing = true;
            SetControlsEnabled(false);

            try
            {
                await _exportService.ExportAsync(selectedItems, dlg.FileName, format);
                MessageBox.Show($"Exportação concluída!\n{selectedItems.Count} arquivos salvos.", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao exportar: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                isProcessing = false;
                SetControlsEnabled(true);
            }
        }

        private void BtnPreview_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = fileTreeView.GetSelectedItems();
            if (!selectedItems.Any())
            {
                MessageBox.Show("Selecione pelo menos um arquivo para visualizar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var previewWindow = new Window
            {
                Title = "Pré-visualização da Seleção",
                Width = 700,
                Height = 500,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            var textBox = new TextBox
            {
                IsReadOnly = true,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                Padding = new Thickness(10)
            };

            var previewContent = new StringBuilder();
            previewContent.AppendLine($"Arquivos selecionados ({selectedItems.Count}):");
            previewContent.AppendLine("=".Repeat(50));
            previewContent.AppendLine();

            foreach (var item in selectedItems.OrderBy(i => i.RelativePath))
            {
                previewContent.AppendLine($"- {item.RelativePath}");
            }

            textBox.Text = previewContent.ToString();
            previewWindow.Content = textBox;
            previewWindow.Owner = this;
            previewWindow.Show();
        }

        private void SetControlsEnabled(bool enabled)
        {
            btnExport.IsEnabled = enabled && !isProcessing;
            btnBrowse.IsEnabled = enabled;
            btnSelectProjectType.IsEnabled = enabled;
            btnPreview.IsEnabled = enabled;
            cmbExportFormat.IsEnabled = enabled;
        }
    }
}