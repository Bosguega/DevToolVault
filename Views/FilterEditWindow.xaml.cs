using System.Windows;
using DevToolVault.Filters;

namespace DevToolVault.Views
{
    public partial class FilterEditWindow : Window
    {
        public string ProfileName { get; private set; }
        public string ProfileDescription { get; private set; }

        public FilterEditWindow()
        {
            InitializeComponent();
        }

        public FilterEditWindow(FilterProfile profile)
        {
            InitializeComponent();
            txtName.Text = profile.Name;
            txtDescription.Text = profile.Description;
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("O nome do perfil não pode estar vazio.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ProfileName = txtName.Text.Trim();
            ProfileDescription = txtDescription.Text.Trim();

            this.DialogResult = true;
            this.Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}