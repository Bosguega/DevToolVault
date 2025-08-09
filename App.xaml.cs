using System;
using System.Windows;

namespace DevToolVault
{
    public partial class App : Application
    {
        public App()
        {
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"Erro não tratado:\n\n{e.Exception.GetType().Name}: {e.Exception.Message}",
                "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true; // evita fechamento do app
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                var ex = e.ExceptionObject as Exception;
                MessageBox.Show($"Erro fatal não tratado:\n\n{ex?.GetType().Name}: {ex?.Message}",
                    "Erro Fatal", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch
            {
                // evita recursão
            }
        }
    }
}