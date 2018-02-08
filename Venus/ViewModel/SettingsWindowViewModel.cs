using System;
using System.Collections.Generic;
using GalaSoft.MvvmLight.Command;
using Venus.Infrastructure;
using Venus.Infrastructure.Events;
using Venus.Infrastructure.Events.EventClasses;
using Venus.Infrastructure.Exchanges;
using Venus.Model;

namespace Venus.ViewModel
{
    public class SettingsWindowViewModel : RequestCloseViewModel
    {
        #region Backing Fields
        private ComboxValuePair _selectedRateValue;
        private RelayCommand _saveCmd;
        private RelayCommand _closeCmd;
        private string _bittrexApiKey;
        private string _gdaxApiKey;
        private string _binanceApiKey;
        private string _gdaxApiPassword;
        private string _bittrexApiSecret;
        private string _gdaxApiSecret;
        private string _binanceApiSecret;
        #endregion

        #region ComboBox Fields and Properties
        private readonly List<ComboxValuePair> _values = new List<ComboxValuePair>
        {
            new ComboxValuePair("5 Seconds", 5),
            new ComboxValuePair("10 Seconds", 10),
            new ComboxValuePair("15 Seconds", 15),
            new ComboxValuePair("20 Seconds", 20),
            new ComboxValuePair("30 Seconds", 30),
            new ComboxValuePair("60 Seconds", 60),
            new ComboxValuePair("2 Minutes", 120),
            new ComboxValuePair("5 Minutes", 300),
            new ComboxValuePair("10 Minutes", 600),
            new ComboxValuePair("15 Minutes", 900),
            new ComboxValuePair("30 Minutes", 1800)

        };
        public List<ComboxValuePair> Values => _values;

        public ComboxValuePair SelectedRateValue
        {
            get => _selectedRateValue ?? (_selectedRateValue = _values[4]);
            set
            {
                _selectedRateValue = value;
                RaisePropertyChanged();
            }
        }
        #endregion
        
        #region Command Properties
        public RelayCommand SaveCommand
        {
            get { return _saveCmd ?? (_saveCmd = new RelayCommand(Save, () => true)); }
        }

        public RelayCommand CloseCommand
        {
            get { return _closeCmd ?? (_closeCmd = new RelayCommand(Close, () => true)); }
        }
        #endregion
        
        #region Bittrex Properties
        public string BittrexApiKey
        {
            get => _bittrexApiKey;
            set
            {
                _bittrexApiKey = value;
                RaisePropertyChanged();
            }
        }

        public string BittrexApiSecret
        {
            get => _bittrexApiSecret;
            set
            {
                _bittrexApiSecret = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region Gdax Properties
        public string GdaxApiKey
        {
            get => _gdaxApiKey;
            set
            {
                _gdaxApiKey = value;
                RaisePropertyChanged();
            }
        }

        public string GdaxApiPassword
        {
            get => _gdaxApiPassword;
            set
            {
                _gdaxApiPassword = value;
                RaisePropertyChanged();
            }
        }

        public string GdaxApiSecret
        {
            get => _gdaxApiSecret;
            set
            {
                _gdaxApiSecret = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region Binance Properties
        public string BinanceApiKey
        {
            get => _binanceApiKey;
            set
            {
                _binanceApiKey = value;
                RaisePropertyChanged();
            }
        }

        public string BinanceApiSecret
        {
            get => _binanceApiSecret;
            set
            {
                _binanceApiSecret = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region Dependency fields

        private readonly IEventPublisher _publisher;
        private readonly ConfigurationManager _configManager;

        #endregion

        #region Methods
        public SettingsWindowViewModel(IEventPublisher publisher, ConfigurationManager configmgr)
        {
            _publisher = publisher;
            _configManager = configmgr;

            ReadApiKeyDataFromConfig(_configManager.CurrentConfiguration);
        }

        private void ReadApiKeyDataFromConfig(Configuration config)
        {
            if (config == null) { return; }

            if (config.BinanceApiKeyData != null)
            {
                BinanceApiKey = config.BinanceApiKeyData.GetRawApiKey();
                BinanceApiSecret = config.BinanceApiKeyData.GetRawApiSecret();
            }

            if (config.BittrexApiKeyData != null)
            {
                BittrexApiKey = config.BittrexApiKeyData.GetRawApiKey();
                BittrexApiSecret = config.BittrexApiKeyData.GetRawApiSecret();
            }

            if (config.GdaxApiKeyData != null)
            {
                GdaxApiKey = config.GdaxApiKeyData.GetRawApiKey();
                GdaxApiSecret = config.GdaxApiKeyData.GetRawApiSecret();
                GdaxApiPassword = config.GdaxApiKeyData.GetRawApiPassword();
            }
        }

        public void Save()
        {
            var binancekey = new ApiKeyData(SupportedExchanges.Binance, _binanceApiKey, _binanceApiSecret, "");
            var bittrexkey = new ApiKeyData(SupportedExchanges.Bittrex, _bittrexApiKey, _bittrexApiSecret, "");
            var gdaxkey = new ApiKeyData(SupportedExchanges.GDax, _gdaxApiKey, _gdaxApiSecret, _gdaxApiPassword);

            var newconfig = new Configuration
            {
                BinanceApiKeyData = binancekey,
                BittrexApiKeyData = bittrexkey,
                GdaxApiKeyData = gdaxkey,
                UpdateInterval = _selectedRateValue.IntValue
            };


            _publisher.Publish(new UserSettingsUpdatedEvent(this, newconfig));

        }

        public void ClearApiKeyData()
        {
            var binancekey = new ApiKeyData(SupportedExchanges.Binance, "", "", "");
            var bittrexkey = new ApiKeyData(SupportedExchanges.Bittrex, "", "", "");
            var gdaxkey = new ApiKeyData(SupportedExchanges.GDax, "", "", "");

            var newconfig = new Configuration
            {
                BinanceApiKeyData = binancekey,
                BittrexApiKeyData = bittrexkey,
                GdaxApiKeyData = gdaxkey,
                UpdateInterval = _selectedRateValue.IntValue
            };

            _publisher.Publish(new UserSettingsUpdatedEvent(this, newconfig));
        }

        public void Close()
        {
            OnRequestClose(EventArgs.Empty);
        }
        #endregion

    }
}