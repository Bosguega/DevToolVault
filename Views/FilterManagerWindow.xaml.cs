using System;
using System.Linq;
using System.Windows;
using DevToolVault.Filters;

namespace DevToolVault.Views
{
    public partial class FilterManagerWindow : Window
    {
        private readonly FileFilterManager _filterManager;

        public FilterManagerWindow(FileFilterManager filterManager)
        {
            InitializeComponent();
            _filterManager = filterManager;
            LoadProfiles();
        }

        private void LoadProfiles()
        {
            lstProfiles.ItemsSource = _filterManager.GetProfiles();

            // Seleciona o perfil ativo
            var activeProfile = _filterManager.GetActiveProfile();
            if (activeProfile != null)
            {
                foreach (var item in lstProfiles.Items)
                {
                    if (item is FilterProfile profile && profile.Name == activeProfile.Name)
                    {
                        lstProfiles.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        private void BtnNew_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FilterEditWindow();
            dialog.Owner = this;

            if (dialog.ShowDialog() == true)
            {
                var newProfile = _filterManager.CreateProfile(dialog.ProfileName, dialog.ProfileDescription);
                _filterManager.SaveProfile(newProfile);
                LoadProfiles();
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (lstProfiles.SelectedItem is FilterProfile selectedProfile)
            {
                var dialog = new FilterEditWindow(selectedProfile);
                dialog.Owner = this;

                if (dialog.ShowDialog() == true)
                {
                    selectedProfile.Name = dialog.ProfileName;
                    selectedProfile.Description = dialog.ProfileDescription;
                    _filterManager.SaveProfile(selectedProfile);
                    LoadProfiles();
                }
            }
            else
            {
                MessageBox.Show("Selecione um perfil para editar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (lstProfiles.SelectedItem is FilterProfile selectedProfile)
            {
                if (selectedProfile.Name == "Default")
                {
                    MessageBox.Show("Não é possível excluir o perfil padrão.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show($"Deseja excluir o perfil '{selectedProfile.Name}'?",
                    "Confirmar Exclusão", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _filterManager.DeleteProfile(selectedProfile);
                    LoadProfiles();
                }
            }
            else
            {
                MessageBox.Show("Selecione um perfil para excluir.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnSetActive_Click(object sender, RoutedEventArgs e)
        {
            if (lstProfiles.SelectedItem is FilterProfile selectedProfile)
            {
                _filterManager.SetActiveProfile(selectedProfile);
                MessageBox.Show($"Perfil '{selectedProfile.Name}' definido como ativo.", "Sucesso",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Selecione um perfil para definir como ativo.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}