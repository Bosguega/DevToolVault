using Ookii.Dialogs.Wpf;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using DevToolVault.Models;
using DevToolVault.Filters;

namespace DevToolVault.Views
{
    public partial class EstruturaWindow : Window
    {
        private bool isProcessing = false;
        private readonly FileFilterManager _filterManager;
        private TreeGenerator _treeGenerator;
        private readonly FileStatistics _statistics;

        public EstruturaWindow(FileFilterManager filterManager = null)
        {
            InitializeComponent();

            // Inicializa o gerenciador de filtros
            _filterManager = filterManager ?? new FileFilterManager();

            // Inicializa as dependências com o perfil ativo
            var activeProfile = _filterManager.GetActiveProfile();
            var options = TreeOptions.FromFilterProfile(activeProfile);
            var fileFilter = new FileFilter(options);
            _statistics = new FileStatistics();
            _treeGenerator = new TreeGenerator(fileFilter, _statistics);

            // Configura a UI com base no perfil ativo
            SetupUIFromProfile(activeProfile);
        }

        private void SetupUIFromProfile(FilterProfile profile)
        {
            chkIgnoreEmptyFolders.IsChecked = profile.IgnoreEmptyFolders;
            chkShowSystemFiles.IsChecked = profile.ShowSystemFiles;
            chkShowOnlyCodeFiles.IsChecked = profile.ShowOnlyCodeFiles;
        }

        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog();
            if (dialog.ShowDialog(this) == true)
            {
                txtFolderPath.Text = dialog.SelectedPath;
                _ = GerarEstruturaAsync();
            }
        }

        private async void BtnGenerate_Click(object sender, RoutedEventArgs e)
        {
            await GerarEstruturaAsync();
        }

        private async Task GerarEstruturaAsync()
        {
            if (isProcessing) return;
            isProcessing = true;

            txtStructure.Text = "Gerando estrutura...";
            SetControlsEnabled(false);
            _statistics.Reset();

            string caminhoSelecionado = txtFolderPath.Text;
            if (string.IsNullOrWhiteSpace(caminhoSelecionado) || !Directory.Exists(caminhoSelecionado))
            {
                MessageBox.Show("Selecione uma pasta válida.", "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
                isProcessing = false;
                SetControlsEnabled(true);
                return;
            }

            try
            {
                // Atualiza o perfil ativo com base nas opções da UI
                var activeProfile = _filterManager.GetActiveProfile();
                activeProfile.IgnoreEmptyFolders = chkIgnoreEmptyFolders.IsChecked == true;
                activeProfile.ShowSystemFiles = chkShowSystemFiles.IsChecked == true;
                activeProfile.ShowOnlyCodeFiles = chkShowOnlyCodeFiles.IsChecked == true;

                var options = TreeOptions.FromFilterProfile(activeProfile);

                // Atualiza o filtro com as novas opções
                var fileFilter = new FileFilter(options);
                _treeGenerator = new TreeGenerator(fileFilter, _statistics);

                string result = await Task.Run(() =>
                {
                    return _treeGenerator.GenerateTree(caminhoSelecionado, options);
                });

                txtStructure.Text = result;
            }
            catch (Exception ex)
            {
                txtStructure.Text = $"Erro: {ex.Message}";
                MessageBox.Show($"Erro ao gerar estrutura: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                isProcessing = false;
                SetControlsEnabled(true);
            }
        }

        private void SetControlsEnabled(bool enabled)
        {
            btnGenerate.IsEnabled = enabled;
            btnBrowse.IsEnabled = enabled;
            chkIgnoreEmptyFolders.IsEnabled = enabled;
            if (chkShowSystemFiles != null)
                chkShowSystemFiles.IsEnabled = enabled;
            if (chkShowOnlyCodeFiles != null)
                chkShowOnlyCodeFiles.IsEnabled = enabled;
            btnCopy.IsEnabled = enabled;
            btnSave.IsEnabled = enabled;
        }

        private void BtnCopy_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtStructure.Text))
            {
                Clipboard.SetText(txtStructure.Text);
                MessageBox.Show("Estrutura copiada para a área de transferência.", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtStructure.Text))
            {
                MessageBox.Show("Nada para salvar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Arquivo de Texto (*.txt)|*.txt",
                FileName = "Estrutura.txt"
            };

            if (dlg.ShowDialog(this) == true)
            {
                try
                {
                    File.WriteAllText(dlg.FileName, txtStructure.Text);
                    MessageBox.Show("Arquivo salvo com sucesso.", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao salvar arquivo: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Eventos para os checkboxes
        private void chkIgnoreEmptyFolders_Checked(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtFolderPath.Text))
                _ = GerarEstruturaAsync();
        }

        private void chkIgnoreEmptyFolders_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtFolderPath.Text))
                _ = GerarEstruturaAsync();
        }

        private void chkShowSystemFiles_Checked(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtFolderPath.Text))
                _ = GerarEstruturaAsync();
        }

        private void chkShowSystemFiles_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtFolderPath.Text))
                _ = GerarEstruturaAsync();
        }

        private void chkShowOnlyCodeFiles_Checked(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtFolderPath.Text))
                _ = GerarEstruturaAsync();
        }

        private void chkShowOnlyCodeFiles_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtFolderPath.Text))
                _ = GerarEstruturaAsync();
        }
    }
}