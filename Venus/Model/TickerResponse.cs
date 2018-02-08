using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Venus.Model
{
    public class TickerResponse : INotifyPropertyChanged
    {
        private string _price;
        private string _size;
        private string _bid;
        private string _ask;
        private string _volume;
        private string _time;

        #region Properties
        public string Price
        {
            get => _price;
            set
            {
                _price = value;
                OnPropertyChanged();
            }
        }

        public string Size
        {
            get => _size;
            set
            {
                _size = value;
                OnPropertyChanged();
            }
        }

        public string Bid
        {
            get => _bid;
            set
            {
                _bid = value;
                OnPropertyChanged();
            }
        }

        public string Ask
        {
            get => _ask;
            set
            {
                _ask = value;
                OnPropertyChanged();
            }
        }

        public string Volume
        {
            get => _volume;
            set
            {
                _volume = value;
                OnPropertyChanged();
            }
        }

        public string Time
        {
            get => _time;
            set
            {
                _time = value;
                OnPropertyChanged();
            }
        }
        #endregion


        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}