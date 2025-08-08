using System.Windows;
using System.Windows.Input;
using DevToolVault.Filters;

namespace DevToolVault.Views
{
    public partial class StartWindow : Window
    {
        private FileFilterManager _filterManager;

        public StartWindow()
        {
            InitializeComponent();
            // Inicializa o gerenciador de filtros
            _filterManager = new FileFilterManager();
            // Atualiza o menu com o filtro ativo
            UpdateFiltroAtual();
        }

        private void UpdateFiltroAtual()
        {
            var activeProfile = _filterManager.GetActiveProfile();
            if (activeProfile != null)
            {
                menuFiltroAtual.Header = $"Filtro Atual: {activeProfile.Name}";
            }
        }

        private void MenuVisualizarEstrutura_Click(object sender, RoutedEventArgs e)
        {
            var estruturaWindow = new EstruturaWindow(_filterManager);
            estruturaWindow.Owner = this;
            estruturaWindow.Show();
            UpdateFiltroAtual(); // Atualiza após fechar a janela
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

        private void MenuGerenciarFiltros_Click(object sender, RoutedEventArgs e)
        {
            var filterWindow = new FilterManagerWindow(_filterManager);
            filterWindow.Owner = this;
            filterWindow.ShowDialog();
            UpdateFiltroAtual(); // Atualiza após fechar a janela
        }

        private void MenuRecarregarFiltros_Click(object sender, RoutedEventArgs e)
        {
            _filterManager = new FileFilterManager();
            UpdateFiltroAtual();
            MessageBox.Show("Filtros recarregados com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MenuSobre_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("DevToolVault v1.0\n\nFerramentas de desenvolvimento em um só lugar.\n\nDesenvolvido por: Seu Nome",
                          "Sobre", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Eventos para os cards
        private void BorderEstrutura_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MenuVisualizarEstrutura_Click(sender, e);
        }

        private void BorderExportar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MenuExportarCodigo_Click(sender, e);
        }

        private void BtnEstrutura_Click(object sender, RoutedEventArgs e)
        {
            MenuVisualizarEstrutura_Click(sender, e);
        }

        private void BtnExportar_Click(object sender, RoutedEventArgs e)
        {
            MenuExportarCodigo_Click(sender, e);
        }
    }
}