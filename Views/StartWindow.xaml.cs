using System.Windows;
using System.Windows.Controls;
using DevToolVault.Views;

namespace DevToolVault.Views
{
    public partial class StartWindow : Window
    {
        public StartWindow()
        {
            InitializeComponent();

            // Carregar a página inicial (ex: Estrutura)
            MainFrame.Navigate(new EstruturaPage());
        }

        private void MenuItem_Estrutura_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new EstruturaPage());
        }

        private void MenuItem_ExportarCodigo_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ExportarCodigoPage());
        }

        private void MenuItem_Sair_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MenuItem_Sobre_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("DevToolVault - Coleção de utilitários para desenvolvedores.\n\nVersão 1.0", "Sobre");
        }
    }
}
