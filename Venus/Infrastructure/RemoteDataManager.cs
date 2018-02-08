using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Venus.Infrastructure.Events;
using Venus.Infrastructure.Events.EventClasses;
using Venus.Infrastructure.Exchanges;
using Venus.Model;

namespace Venus.Infrastructure
{
    public class RemoteDataManager
    {
        public static decimal CurrentBitcoinPrice { get; private set; }

        private readonly StaTaskScheduler _staTaskScheduler;
        private readonly List<IExchangeWrapper> _exchanges;
        private readonly IEventPublisher _publisher;

        public RemoteDataManager(IEventPublisher publisher)
        {
            CurrentBitcoinPrice = 0;
            _staTaskScheduler = new StaTaskScheduler(Environment.ProcessorCount);
            _exchanges = new List<IExchangeWrapper>();
            _publisher = publisher;

            _publisher.GetEvent<UserSettingsUpdatedEvent>().Subscribe(OnSettingsUpdate);
        }

        private void LoadExchanges()
        {
            _exchanges.Add(new GdaxWrapper());
            _exchanges.Add(new BittrexExchangeWrapper());
            _exchanges.Add(new BinanceWrapper());

            _exchanges.FirstOrDefault(
                x => x.Exchange == SupportedExchanges.GDax)?
                .Init(App.ConfigurationManager.CurrentConfiguration.GdaxApiKeyData);

            _exchanges.FirstOrDefault(
                x => x.Exchange == SupportedExchanges.Bittrex)?
                .Init(App.ConfigurationManager.CurrentConfiguration.BittrexApiKeyData);

            _exchanges.FirstOrDefault(
                x => x.Exchange == SupportedExchanges.Binance)?
                .Init(App.ConfigurationManager.CurrentConfiguration.BinanceApiKeyData);
        }

        public void Init()
        {
            LoadExchanges();

            _publisher.GetEvent<TimerTickElapsedEvent>().Subscribe(OnTickEvent);
            _publisher.GetEvent<UserSettingsUpdatedEvent>().Subscribe(OnSettingsUpdate);
        }

        private void UpdateMarkets()
        {
            foreach (var exchange in _exchanges)
            {
                exchange.UpdateMarketSummary();
            }
        }

        private void OnSettingsUpdate(UserSettingsUpdatedEvent e)
        {
            if (e.Sender == this) { return; }
            if(e.NewConfiguration == null) { return; }

            _exchanges.FirstOrDefault(
                x => x.Exchange == SupportedExchanges.GDax)?
                .Init(e.NewConfiguration.GdaxApiKeyData);

            _exchanges.FirstOrDefault(
                x => x.Exchange == SupportedExchanges.Bittrex)?
                .Init(e.NewConfiguration.BittrexApiKeyData);

            _exchanges.FirstOrDefault(
                x => x.Exchange == SupportedExchanges.Binance)?
                .Init(e.NewConfiguration.BinanceApiKeyData);
        }

        private void OnTickEvent(TimerTickElapsedEvent tickElapsedEvent)
        {
            _publisher.Publish(new RefreshStatusChangeEvent(true));
            UpdateAllAccounts();
        }

        public void UpdateAllAccounts()
        {
            Task.Factory.StartNew(() =>
            {
                UpdateCurrentBitcoinPrice();
                UpdateMarkets();

                var coinList = GetAllCoinsForAllAccounts();

                _publisher.Publish(new AccountUpdatedEvent
                {
                    UpdatedCoinList = coinList
                });

                _publisher.Publish(new RefreshStatusChangeEvent(false));
            }, CancellationToken.None, TaskCreationOptions.None, _staTaskScheduler);
        }

        public List<Coin> GetAllCoinsForAllAccounts()
        {
            var balances = new List<Coin>();

            foreach (var exchange in _exchanges)
            {
                if (!exchange.IsConfigured) {continue;}

                var data = exchange.GetAccountData();

                foreach (var coin in data)
                {
                    var c = balances.FirstOrDefault(x => x.Symbol == coin.Symbol);
                    if (c == null)
                    {
                        balances.Add(coin);
                        continue;
                    }

                    c.Balance += coin.Balance;
                    c.BtcValue += coin.BtcValue;
                }
            }

            return balances;
        }

        public void UpdateCurrentBitcoinPrice()
        {
            var response = _exchanges.FirstOrDefault(
                               x => x.Exchange == SupportedExchanges.GDax)?
                               .GetBitcoinPrice().Result ?? 0;

            CurrentBitcoinPrice = response;
        }
    }
}