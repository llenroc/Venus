using System.ComponentModel;
using System.Runtime.CompilerServices;
using Venus.Infrastructure;

namespace Venus.Model
{
    public class Coin : INotifyPropertyChanged
    {
        private string _symbol;
        private string _price;
        private decimal _balance;
        private decimal _btcValue;

        public string Symbol
        {
            get => _symbol;
            set
            {
                _symbol = value; 
                OnPropertyChanged();
            }
        }

        public string RawPriceString => _price;

        public string Price
        {
            get => $"Price: {_price}";
            set
            {
                _price = value;
                OnPropertyChanged();
            }
        }

        public decimal Balance
        {
            get => _balance;
            set 
            { 
                _balance = value; 
                OnPropertyChanged();
                OnPropertyChanged("BalanceText");
                OnPropertyChanged("BtcValue");
                OnPropertyChanged("BtcValueText");
                OnPropertyChanged("DollarValue");
                OnPropertyChanged("DollarValueText");
            }
        }
        public string BalanceText => $"Balance: {Balance:0.########}";

        public decimal BtcValue
        {
            get => _btcValue;
            set
            {
                _btcValue = value;
                OnPropertyChanged();
                OnPropertyChanged("BtcValueText");
                OnPropertyChanged("DollarValue");
                OnPropertyChanged("DollarValueText");
            }
        }
        public string BtcValueText => $"BTC Value: {BtcValue:0.########}";
        
        public decimal DollarValue => RemoteDataManager.CurrentBitcoinPrice * BtcValue;
        public string DollarValueText => $"Value: ${(RemoteDataManager.CurrentBitcoinPrice * BtcValue):0.##}";

        public void ClearCoinBalanceData()
        {
            Balance = 0;
            BtcValue = 0;
        }

        public void CopyCoinData(Coin source)
        {
            Balance = source.Balance;
            BtcValue = source.BtcValue;
            Price = source.RawPriceString;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}