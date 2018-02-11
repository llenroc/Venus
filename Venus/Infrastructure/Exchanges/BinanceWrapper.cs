using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinanceExchange.API.Client;
using BinanceExchange.API.Models.Response;
using Venus.Model;

namespace Venus.Infrastructure.Exchanges
{
    public class BinanceWrapper : IExchangeWrapper
    {
        public SupportedExchanges Exchange { get; private set; }
        public bool IsConfigured { get; private set; }
        private List<SymbolPriceResponse> _summary;
        private ApiKeyData _apiKey;
        private BinanceClient _client;

        public BinanceWrapper()
        {
            Exchange = SupportedExchanges.Binance;
            _summary = new List<SymbolPriceResponse>();
        }

        public void Init(ApiKeyData keyData)
        {
            if (string.IsNullOrWhiteSpace(keyData.GetRawApiKey()))
            {
                IsConfigured = false;
                return;
            }

            UpdateKeyData(keyData);

            UpdateMarketSummary();
        }

        public void UpdateKeyData(ApiKeyData keyData)
        {
            _apiKey = keyData;

            _client = new BinanceClient(new ClientConfiguration()
            {
                ApiKey = _apiKey.GetRawApiKey(),
                SecretKey = _apiKey.GetRawApiSecret()
            });

            IsConfigured = true;
        }

        public void UpdateMarketSummary()
        {
            if (_apiKey == null || !IsConfigured) { return; }

            if (_client == null)
            {
                _client = new BinanceClient(new ClientConfiguration()
                {
                    ApiKey = _apiKey.GetRawApiKey(),
                    SecretKey = _apiKey.GetRawApiSecret()
                });
            }

            _summary = Task.Run(() => _client.GetSymbolsPriceTicker()).Result;

            // Occasionally, the Binance API wrapper will return 0 results
            // without an error. If it returns 0, pull last ask price from
            // order book and use that for the market summary

            if (_summary.Count != 0)
            {
                return;
            }

            var q = Task.Run(() => _client.GetSymbolOrderBookTicker()).Result;

            foreach (var resp in q)
            {
                var btpr = new SymbolPriceResponse()
                {
                    Price = resp.AskPrice,
                    Symbol = resp.Symbol
                };

                _summary.Add(btpr);
            }
        }
        
        public List<Coin> GetAccountData()
        {
            var coinList = new List<Coin>();

            _client.TimestampOffset = TimeSpan.FromMilliseconds(-1000);
            var account = _client.GetAccountInformation().Result.Balances;

            foreach (var balance in account)
            {
                var coinBalance = balance.Free + balance.Locked;
                if (coinBalance <= 0) { continue; }

                var newCoin = new Coin
                {
                    Symbol = balance.Asset,
                    Balance = coinBalance,
                    Price = $"BTC {GetCoinBtcValue(balance.Asset):0.########}"
                };

                if (newCoin.Symbol != "BTC")
                {
                    newCoin.BtcValue +=
                        coinBalance * GetCoinBtcValue(balance.Asset);
                    coinList.Add(newCoin);
                    continue;
                }

                newCoin.BtcValue = newCoin.Balance;
                coinList.Add(newCoin);
            }

            return coinList;
        }

        public Task<decimal> GetBitcoinPrice()
        {
            return Task.Run(
                () => 
                    _summary.FirstOrDefault(x => x.Symbol == $"BTCUSDT")?.Price ?? 0);
        }

        public decimal GetCoinBtcValue(string coin)
        {
            return _summary.FirstOrDefault(x => x.Symbol == $"{coin}BTC")?.Price ?? 0;
        }
    }
}