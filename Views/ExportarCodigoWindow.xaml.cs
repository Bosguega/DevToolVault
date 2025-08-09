using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using DevToolVault.Controls;
using DevToolVault.Filters;
using DevToolVault.Models;
using Ookii.Dialogs.Wpf;

namespace DevToolVault.Views
{
    public partial class ExportarCodigoWindow : Window
    {
        private bool isProcessing = false;
        private readonly FileFilterManager _filterManager;
        private string _projectRoot;

        public ExportarCodigoWindow(FileFilterManager filterManager = null)
        {
            InitializeComponent();

            // Inicializa o gerenciador de filtros
            _filterManager = filterManager ?? new FileFilterManager();

            // Se não há perfil ativo ou é o perfil genérico, pede para selecionar
            var activeProfile = _filterManager.GetActiveProfile();
            if (activeProfile == null || activeProfile.Name == "Default")
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
                    // Se o usuário cancelou, usa o perfil padrão
                    activeProfile = _filterManager.GetProfiles().FirstOrDefault(p => p.Name == "Flutter") ??
                                   _filterManager.GetProfiles().FirstOrDefault();
                    _filterManager.SetActiveProfile(activeProfile);
                }
            }

            // Configura a UI com base no perfil ativo
            SetupUIFromProfile(activeProfile);
            txtCurrentProfile.Text = $"Perfil atual: {activeProfile.Name}";
        }

        private void SetupUIFromProfile(FilterProfile profile)
        {
            txtCurrentProfile.Text = $"Perfil atual: {profile.Name}";
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

                    // Recarrega a árvore se já tiver um caminho
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

            isProcessing = true;
            SetControlsEnabled(false);

            try
            {
                var exportContent = new StringBuilder();

                foreach (var item in selectedItems)
                {
                    try
                    {
                        // Adiciona o cabeçalho
                        exportContent.AppendLine($"// Caminho: {item.RelativePath}");
                        exportContent.AppendLine($"// Arquivo: {item.Name}");
                        exportContent.AppendLine("//");

                        // Lê o conteúdo do arquivo
                        string content = File.ReadAllText(item.FullName);
                        exportContent.AppendLine(content);

                        // Adiciona um separador
                        exportContent.AppendLine();
                        exportContent.AppendLine("=".Repeat(80));
                        exportContent.AppendLine();
                    }
                    catch (Exception ex)
                    {
                        exportContent.AppendLine($"// Erro ao ler arquivo {item.RelativePath}: {ex.Message}");
                        exportContent.AppendLine("=".Repeat(80));
                        exportContent.AppendLine();
                    }
                }

                // Salva o arquivo
                var dlg = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Arquivo de Texto (*.txt)|*.txt",
                    FileName = "codigo_selecionado.txt"
                };

                if (dlg.ShowDialog(this) == true)
                {
                    File.WriteAllText(dlg.FileName, exportContent.ToString());
                    MessageBox.Show($"Exportação concluída! {selectedItems.Count} arquivos exportados.",
                                  "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro durante a exportação: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
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
            btnExport.IsEnabled = enabled;
            btnBrowse.IsEnabled = enabled;
            btnSelectProjectType.IsEnabled = enabled;
            btnPreview.IsEnabled = enabled;
        }
    }
}

// Extensão para repetir strings
public static class StringExtensions
{
    public static string Repeat(this string input, int count)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        var builder = new StringBuilder(input.Length * count);
        for (int i = 0; i < count; i++)
        {
            builder.Append(input);
        }
        return builder.ToString();
    }
}