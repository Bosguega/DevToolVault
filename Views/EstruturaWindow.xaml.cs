using Ookii.Dialogs.Wpf;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using DevToolVault.Models;

namespace DevToolVault.Views
{
    public partial class EstruturaWindow : Window
    {
        private readonly string[] defaultIgnorePatterns = new string[]
        {
            ".git", ".svn", ".hg", ".vs", "bin", "obj",
            "node_modules", "Debug", "Release", "packages",
            "Thumbs.db", "desktop.ini", "$RECYCLE.BIN",
            "System Volume Information",
            "build", ".gradle", ".idea", "gradle", "captures",
            "local.properties", "*.iml", ".cxx", "lint",
            "dist", "out", "generated", ".externalNativeBuild"
        };

        private bool isProcessing = false;
        private readonly TreeGenerator _treeGenerator;
        private readonly FileStatistics _statistics;

        public EstruturaWindow()
        {
            InitializeComponent();

            // Inicializa as dependências
            var options = new TreeOptions
            {
                IgnorePatterns = defaultIgnorePatterns
            };
            var fileFilter = new FileFilter(options);
            _statistics = new FileStatistics();
            _treeGenerator = new TreeGenerator(fileFilter, _statistics);
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

            // Atualizações iniciais na UI thread
            txtStructure.Text = "Gerando estrutura...";
            SetControlsEnabled(false);

            // Resetar estatísticas
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
                // Prepara as opções com base nos controles da UI
                var options = new TreeOptions
                {
                    IgnoreEmptyFolders = chkIgnoreEmptyFolders.IsChecked == true,
                    ShowFileSize = false, // Pode adicionar um checkbox para isso
                    ShowSystemFiles = chkShowSystemFiles?.IsChecked == true, // Operador seguro para nulo
                    IgnorePatterns = defaultIgnorePatterns
                };

                // Executa a tarefa em segundo plano
                string result = await Task.Run(() =>
                {
                    return _treeGenerator.GenerateTree(caminhoSelecionado, options);
                });

                // Atualiza a UI após o await (volta para a thread principal)
                txtStructure.Text = result;
                // Se você tiver um controle para estatísticas, atualize aqui
                // Ex: txtStats.Text = _statistics.ToString();
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
            if (chkShowSystemFiles != null) // Verifica se o controle existe
                chkShowSystemFiles.IsEnabled = enabled;
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
    }
}