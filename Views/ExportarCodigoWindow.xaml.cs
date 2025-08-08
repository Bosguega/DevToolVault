using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Ookii.Dialogs.Wpf;

namespace DevToolVault.Views
{
    public partial class ExportarCodigoWindow : Window
    {
        // Extensões de arquivo que consideramos como código
        private readonly string[] codeExtensions = {
            ".cs", ".xaml", ".xml", ".json", ".config",
            ".cshtml", ".js", ".html", ".css", ".ts",
            ".vb", ".cpp", ".h", ".py", ".java"
        };

        private List<FileInfo> _projectFiles = new List<FileInfo>();

        public ExportarCodigoWindow()
        {
            InitializeComponent();
        }

        private void BtnBrowseProject_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog();
            if (dialog.ShowDialog(this) == true)
            {
                txtProjectPath.Text = dialog.SelectedPath;
            }
        }

        private void BtnBrowseOutput_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog();
            if (dialog.ShowDialog(this) == true)
            {
                txtOutputPath.Text = dialog.SelectedPath;
            }
        }

        private async void BtnScan_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtProjectPath.Text) || !Directory.Exists(txtProjectPath.Text))
            {
                MessageBox.Show("Selecione uma pasta de projeto válida.", "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            btnScan.IsEnabled = false;
            lstFiles.Items.Clear();
            _projectFiles.Clear();

            try
            {
                await Task.Run(() => ScanProjectFiles());
                lstFiles.ItemsSource = _projectFiles.Select(f => new { f.Name, Path = f.DirectoryName });
                MessageBox.Show($"Encontrados {_projectFiles.Count} arquivos de código.", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao escanear: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                btnScan.IsEnabled = true;
            }
        }

        private void ScanProjectFiles()
        {
            var searchOption = chkIncludeSubfolders.IsChecked == true ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var files = Directory.GetFiles(txtProjectPath.Text, "*.*", searchOption)
                                .Where(f => codeExtensions.Contains(Path.GetExtension(f).ToLower()))
                                .Select(f => new FileInfo(f));

            foreach (var file in files)
            {
                _projectFiles.Add(file);
            }
        }

        private async void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            if (_projectFiles.Count == 0)
            {
                MessageBox.Show("Nenhum arquivo para exportar. Execute a varredura primeiro.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtOutputPath.Text))
            {
                MessageBox.Show("Selecione uma pasta de destino.", "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            btnExport.IsEnabled = false;
            try
            {
                await Task.Run(() => ExportFiles());
                MessageBox.Show("Exportação concluída com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro durante exportação: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                btnExport.IsEnabled = true;
            }
        }

        private void ExportFiles()
        {
            foreach (var file in _projectFiles)
            {
                try
                {
                    // Calcula o caminho relativo do arquivo
                    string relativePath = file.FullName.Substring(txtProjectPath.Text.Length).TrimStart(Path.DirectorySeparatorChar);

                    // Define o caminho de saída
                    string outputPath;
                    if (chkPreserveStructure.IsChecked == true)
                    {
                        // Mantém a estrutura de diretórios
                        outputPath = Path.Combine(txtOutputPath.Text, relativePath);
                    }
                    else
                    {
                        // Salva todos na pasta raiz de destino
                        outputPath = Path.Combine(txtOutputPath.Text, file.Name);
                    }

                    // Garante que o diretório de destino existe
                    Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

                    // Salva o conteúdo do arquivo com extensão .txt
                    File.WriteAllText(Path.ChangeExtension(outputPath, ".txt"), File.ReadAllText(file.FullName));
                }
                catch (Exception ex)
                {
                    // Log de erros específicos por arquivo
                    Console.WriteLine($"Erro ao exportar {file.Name}: {ex.Message}");
                }
            }
        }
    }
}