using System.Windows;
using GalaSoft.MvvmLight;

namespace Venus.MVVM.Services
{
    public class WindowService : IWindowService
    {
        public void ShowWindow<T>(ViewModelBase viewModel) where T : Window, new()
        {
            var win = new T
            {
                DataContext = viewModel
            };
            win.Show();
        }

        public void ShowDialog<T>(ViewModelBase viewModel) where T : Window, new()
        {
            var win = new T
            {
                DataContext = viewModel,
                Owner = Application.Current.MainWindow,
                Topmost = true
            };
            win.ShowDialog();
        }
    }
}