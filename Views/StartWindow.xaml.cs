using System.Windows;

namespace DevToolVault.Views
{
    public partial class StartWindow : Window
    {
        public StartWindow()
        {
            InitializeComponent();
        }

        private void MenuVisualizarEstrutura_Click(object sender, RoutedEventArgs e)
        {
            var estruturaWindow = new EstruturaWindow();
            estruturaWindow.Owner = this;  // Define StartWindow como dona
            estruturaWindow.Show();
        }

        private void MenuExportarCodigo_Click(object sender, RoutedEventArgs e)
        {
            var exportarWindow = new ExportarCodigoWindow();
            exportarWindow.Owner = this;
            exportarWindow.Show();
        }

        private void MenuSair_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
