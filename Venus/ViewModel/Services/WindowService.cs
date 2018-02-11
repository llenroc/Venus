using System.Windows;
using GalaSoft.MvvmLight;
using Venus.View;

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

        public void ShowAboutWindow()
        {
            var win = new About
            {
                Owner = Application.Current.MainWindow,
                Topmost = true
            };

            win.ShowDialog();
        }
    }
}