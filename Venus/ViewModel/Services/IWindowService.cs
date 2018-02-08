using System.Windows;
using GalaSoft.MvvmLight;

namespace Venus.MVVM.Services
{
    public interface IWindowService
    {
        void ShowWindow<T>(ViewModelBase viewModel) where T : Window, new();
        void ShowDialog<T>(ViewModelBase viewModel) where T : Window, new();
    }
}