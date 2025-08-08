using Ookii.Dialogs.Wpf; // Se quiser usar o VistaFolderBrowserDialog
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DevToolVault.Views
{
    public partial class EstruturaPage : Page
    {
        private string pastaSelecionada;

        public EstruturaPage()
        {
            InitializeComponent();
        }

        private void BtnSelecionarPasta_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new VistaFolderBrowserDialog();
            if (dlg.ShowDialog() == true)
            {
                pastaSelecionada = dlg.SelectedPath;
                TxtEstrutura.Text = pastaSelecionada;
            }
        }

        private async void BtnAtualizarEstrutura_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(pastaSelecionada) || !Directory.Exists(pastaSelecionada))
            {
                MessageBox.Show("Selecione uma pasta válida.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            TxtEstrutura.Text = "Gerando estrutura...";
            string estrutura = await Task.Run(() => GerarEstrutura(pastaSelecionada));
            TxtEstrutura.Text = estrutura;
        }

        private string GerarEstrutura(string caminho)
        {
            var sb = new StringBuilder();
            // Aqui você pode colocar a lógica para gerar a estrutura de diretórios
            // Por enquanto só exemplo simples:
            sb.AppendLine(caminho);
            var di = new DirectoryInfo(caminho);
            foreach (var dir in di.GetDirectories())
            {
                sb.AppendLine("├── " + dir.Name + "/");
                foreach (var file in dir.GetFiles())
                {
                    sb.AppendLine("│   ├── " + file.Name);
                }
            }
            return sb.ToString();
        }

        private void BtnCopiar_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(TxtEstrutura.Text))
            {
                Clipboard.SetText(TxtEstrutura.Text);
                MessageBox.Show("Estrutura copiada para a área de transferência.");
            }
        }

        private void BtnExportar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtEstrutura.Text))
            {
                MessageBox.Show("Nada para exportar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Arquivo de Texto (*.txt)|*.txt",
                FileName = "Estrutura.txt"
            };

            if (dlg.ShowDialog() == true)
            {
                File.WriteAllText(dlg.FileName, TxtEstrutura.Text);
                MessageBox.Show("Arquivo exportado com sucesso.");
            }
        }
    }
}
