using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Venus.Infrastructure.Events;
using Venus.Infrastructure.Events.EventClasses;
using Venus.Model;

namespace Venus.Infrastructure
{
    public class Portfolio
    {
        private static Portfolio _instance;
        private readonly IEventPublisher _publisher;

        public static Portfolio Instance
        {
            get => _instance ?? (_instance = new Portfolio(App.IoC.GetInstance<IEventPublisher>()));
            private set => _instance = value;
        }

        public decimal PortfolioDollarValue { get; set; } = 0;
        public string PortfolioDollarValueString => $"${PortfolioDollarValue:0.##}";
        public ObservableCollection<Coin> CoinList { get; }

        public Portfolio(IEventPublisher publisher)
        {
            _publisher = publisher;
            CoinList = new ObservableCollection<Coin>();
            Instance = this;
            _publisher.GetEvent<AccountUpdatedEvent>().Subscribe(OnAccountUpdateComplete);
        }

        private void OnAccountUpdateComplete(AccountUpdatedEvent e)
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                var coinList = e?.UpdatedCoinList;
                if (coinList == null || coinList.Count == 0) { return; }

                lock (CoinList)
                {
                    foreach (var coin in coinList)
                    {
                        var entry = CoinList.FirstOrDefault(x => x.Symbol == coin.Symbol);

                        if (entry == null)
                        {
                            CoinList.Add(coin);
                            continue;
                        }

                        entry.CopyCoinData(coin);
                    }
                }
                
                UpdatePortfolioDollarValue();

                _publisher.Publish(new PortfolioUpdatedEvent(this));
            });
        }

        private void UpdatePortfolioDollarValue()
        {
            PortfolioDollarValue = 0;
            foreach (var coin in CoinList)
            {
                if (coin == null) { continue; }
                PortfolioDollarValue += coin.DollarValue;
            }
        }
    }
}