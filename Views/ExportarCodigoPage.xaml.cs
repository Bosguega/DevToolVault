using Ookii.Dialogs.Wpf;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace DevToolVault.Views
{
    public partial class ExportarCodigoPage : Page
    {
        private string pastaSelecionada;

        public ExportarCodigoPage()
        {
            InitializeComponent();
        }

        private void BtnSelecionarPasta_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new VistaFolderBrowserDialog();
            if (dlg.ShowDialog() == true)
            {
                pastaSelecionada = dlg.SelectedPath;
                TvArquivos.Items.Clear();
            }
        }

        private void BtnGerarLista_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(pastaSelecionada) || !Directory.Exists(pastaSelecionada))
            {
                MessageBox.Show("Selecione uma pasta válida.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            TvArquivos.Items.Clear();

            var rootDir = new DirectoryInfo(pastaSelecionada);
            var rootItem = CriarTreeViewItem(rootDir);
            TvArquivos.Items.Add(rootItem);
        }

        private TreeViewItem CriarTreeViewItem(DirectoryInfo dirInfo)
        {
            var item = new TreeViewItem
            {
                Header = CriarCheckBox(dirInfo.Name, true)
            };

            try
            {
                foreach (var dir in dirInfo.GetDirectories())
                {
                    item.Items.Add(CriarTreeViewItem(dir));
                }

                foreach (var file in dirInfo.GetFiles())
                {
                    var fileItem = new TreeViewItem
                    {
                        Header = CriarCheckBox(file.Name, false)
                    };
                    item.Items.Add(fileItem);
                }
            }
            catch
            {
                // Ignorar erros de acesso
            }

            return item;
        }

        private CheckBox CriarCheckBox(string nome, bool isDirectory)
        {
            var cb = new CheckBox
            {
                Content = nome,
                IsChecked = true,
                Tag = isDirectory
            };
            return cb;
        }

        private void BtnExportarSelecionados_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(pastaSelecionada) || !Directory.Exists(pastaSelecionada))
            {
                MessageBox.Show("Selecione uma pasta válida.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var itensSelecionados = new List<string>();
            foreach (TreeViewItem item in TvArquivos.Items)
            {
                ObterItensSelecionados(item, "", itensSelecionados);
            }

            if (itensSelecionados.Count == 0)
            {
                MessageBox.Show("Nenhum arquivo ou pasta selecionado.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Arquivo de Texto (*.txt)|*.txt",
                FileName = "CodigoSelecionado.txt"
            };

            if (dlg.ShowDialog() == true)
            {
                File.WriteAllLines(dlg.FileName, itensSelecionados);
                MessageBox.Show("Exportação concluída com sucesso!");
            }
        }

        private void ObterItensSelecionados(TreeViewItem item, string path, List<string> lista)
        {
            var cb = item.Header as CheckBox;
            if (cb == null || cb.IsChecked != true)
                return;

            string nomeAtual = cb.Content.ToString();
            bool isDir = (bool)cb.Tag;

            string novoPath = Path.Combine(path, nomeAtual);

            if (isDir)
            {
                lista.Add($"[D] {novoPath}");
                foreach (TreeViewItem subItem in item.Items)
                {
                    ObterItensSelecionados(subItem, novoPath, lista);
                }
            }
            else
            {
                lista.Add(novoPath);
            }
        }
    }
}
