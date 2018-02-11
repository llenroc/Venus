using System;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Venus.Infrastructure.Events;
using Venus.Infrastructure.Events.EventClasses;
using Venus.Model;
using Venus.MVVM.Services;
using Venus.View;

namespace Venus.ViewModel
{
    public class MainViewModel : ViewModelBase
    {

        private readonly IEventPublisher _publisher;
        private readonly IWindowService _windowService;

        private ObservableCollection<Coin> _coinList;
        public ObservableCollection<Coin> CoinList
        {
            get => _coinList;
            set
            {
                _coinList = value;
                RaisePropertyChanged();
            }

        }

        private string _portfolioDollarValueString;
        public string PortfolioDollarValueString
        {
            get => _portfolioDollarValueString;
            set
            {
                _portfolioDollarValueString = value;
                RaisePropertyChanged();
            }
        }

        private RelayCommand _refreshCommand;
        public RelayCommand RefreshCommand
        {
            get
            {
                if (_refreshCommand == null)
                {
                    RefreshCommand = new RelayCommand(() => 
                        {RefreshCommandExecute(null, null);}, 
                        () => true);
                }
                return _refreshCommand;
            }
            set
            {
                _refreshCommand = value;
                RaisePropertyChanged();
            }
        }

        private RelayCommand _openSettingsWindowCommand;
        public RelayCommand OpenSettingsWindowCommand
        {
            get
            {
                if (_openSettingsWindowCommand == null)
                {
                    OpenSettingsWindowCommand = 
                        new RelayCommand(() => 
                            { OpenSettingsWindowExecute(null, null); }, 
                            () => true);
                }
                return _openSettingsWindowCommand;
            }
            set
            {
                _openSettingsWindowCommand = value;
                RaisePropertyChanged();
            }
        }

        private RelayCommand _showAbout;
        public RelayCommand ShowAbout
        {
            get
            {
                if (_showAbout == null)
                {
                    ShowAbout =
                        new RelayCommand(() => { ShowAboutExecute(null, null);},
                            () => true);
                }

                return _showAbout;
            }
            set
            {
                _showAbout = value;
                RaisePropertyChanged();
            }
        }

        private bool _updateStatusMessageVisible = true;
        public bool UpdateStatusMessageVisible
        {
            get => _updateStatusMessageVisible;
            set
            {
                _updateStatusMessageVisible = value;
                RaisePropertyChanged();
            }
        }

        public MainViewModel(IEventPublisher publisher, IWindowService windowService)
        {
            _publisher = publisher;
            _windowService = windowService;

            _publisher.GetEvent<PortfolioUpdatedEvent>().Subscribe(OnPortfolioUpdate);
            _publisher.GetEvent<RefreshStatusChangeEvent>().Subscribe(OnRefreshStatusChange);
        }

        private void RefreshCommandExecute(object sender, EventArgs e)
        {
            App.DataManager.UpdateAllAccounts();
            App.TimerManager.RestartTimer();
        }

        private void ShowAboutExecute(object sender, EventArgs e)
        {
            _windowService.ShowAboutWindow();
        }

        private void OpenSettingsWindowExecute(object sender, EventArgs e)
        {
            _windowService.ShowDialog<SettingsWindow>(
                App.IoC.GetInstance<SettingsWindowViewModel>());
        }

        private void OnPortfolioUpdate(PortfolioUpdatedEvent e)
        {
            CoinList = e.Portfolio.CoinList;
            PortfolioDollarValueString = e.Portfolio.PortfolioDollarValueString;
        }

        private void OnRefreshStatusChange(RefreshStatusChangeEvent e)
        {
            UpdateStatusMessageVisible = e.RefreshCurrentlyActive;
        }
    }
}