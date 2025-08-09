using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using DevToolVault.Filters;
using DevToolVault.Models;
using Ookii.Dialogs.Wpf;

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
                    activeProfile = _filterManager.GetProfiles().FirstOrDefault(p => p.Name == "Flutter") ??
                                   _filterManager.GetProfiles().FirstOrDefault();
                    _filterManager.SetActiveProfile(activeProfile);
                }
            }

            var options = TreeOptions.FromFilterProfile(activeProfile);
            var fileFilter = new FileFilter(options);
            _statistics = new FileStatistics();
            _treeGenerator = new TreeGenerator(fileFilter, _statistics);

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

                    if (!string.IsNullOrWhiteSpace(txtFolderPath.Text))
                    {
                        _ = GerarEstruturaAsync();
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
                var activeProfile = _filterManager.GetActiveProfile();
                var options = TreeOptions.FromFilterProfile(activeProfile);

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
            btnSelectProjectType.IsEnabled = enabled;
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

        private void BtnDebug_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFolderPath.Text) || !Directory.Exists(txtFolderPath.Text))
            {
                MessageBox.Show("Selecione uma pasta válida.", "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var debugWindow = new Window
            {
                Title = "Saída de Depuração",
                Width = 600,
                Height = 400,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            var textBox = new TextBox
            {
                IsReadOnly = true,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                Text = "Iniciando depuração...\r\n\r\n"
            };

            debugWindow.Content = textBox;

            var writer = new TextBoxStreamWriter(textBox);
            Console.SetOut(writer);

            var activeProfile = _filterManager.GetActiveProfile();
            var options = TreeOptions.FromFilterProfile(activeProfile);
            var fileFilter = new FileFilter(options);

            Console.WriteLine($"Testando filtro no diretório: {txtFolderPath.Text}");
            fileFilter.ShouldIgnoreDebug(txtFolderPath.Text, true);

            var rootDir = new DirectoryInfo(txtFolderPath.Text);
            foreach (var dir in rootDir.GetDirectories())
            {
                fileFilter.ShouldIgnoreDebug(dir.FullName, true);
            }

            debugWindow.Owner = this;
            debugWindow.Show();
        }
    }
}