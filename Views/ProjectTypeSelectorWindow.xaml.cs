using System.Windows;
using DevToolVault.Filters;

namespace DevToolVault.Views
{
    public partial class ProjectTypeSelectorWindow : Window
    {
        public FilterProfile SelectedProfile { get; private set; }

        public ProjectTypeSelectorWindow(FileFilterManager filterManager)
        {
            InitializeComponent();
            lstProfiles.ItemsSource = filterManager.GetProfiles();
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            if (lstProfiles.SelectedItem is FilterProfile profile)
            {
                SelectedProfile = profile;
                this.DialogResult = true;
            }
            else
            {
                MessageBox.Show("Selecione um tipo de projeto.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}