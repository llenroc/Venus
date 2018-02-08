using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bittrex.Api.Client;
using Bittrex.Api.Client.Models;
using Venus.Model;

namespace Venus.Infrastructure.Exchanges
{
    public class BittrexExchangeWrapper : IExchangeWrapper
    {
        private ApiKeyData _apiKey;
        private ApiResult<MarketSummary[]> _summaries;
        public bool IsConfigured { get; private set; }
        private BittrexClient _client;

        public SupportedExchanges Exchange { get; }

        public BittrexExchangeWrapper()
        {
            Exchange = SupportedExchanges.Bittrex;
        }

        public void Init(ApiKeyData keyData)
        {
            if (string.IsNullOrWhiteSpace(keyData.GetRawApiKey()))
            {
                IsConfigured = false;
                return;
            }
            _apiKey = keyData;
            IsConfigured = true;

            UpdateMarketSummary();
        }

        public void UpdateKeyData(ApiKeyData keyData)
        {
            if (string.IsNullOrWhiteSpace(keyData.GetRawApiKey()))
            {
                IsConfigured = false;
                return;
            }

            _apiKey = keyData;
            _client = new BittrexClient(
                _apiKey.GetRawApiKey(), 
                _apiKey.GetRawApiSecret());
        }

        public void UpdateMarketSummary()
        {
            if (_apiKey == null || !IsConfigured) { return; }

            if (_client == null)
            {
                _client = new BittrexClient(
                    _apiKey.GetRawApiKey(), 
                    _apiKey.GetRawApiSecret());
            }
            
            _summaries = Task.Run(() => _client.GetMarketSummaries()).Result;

        }

        public List<Coin> GetAccountData()
        {
            var coinList = new List<Coin>();

            ApiResult<AccountBalance[]> account = null;

            try
            {
                account = Task.Run(async () => await _client.GetBalances()).Result;
            }
            catch (Exception e)
            {
                //TODO: Handle Bittrex Api GetBalances() exception
            }

            if (account == null) { return coinList; }

            foreach (var balance in account.Result)
            {
                if (balance.Balance <= 0) { continue; }

                var newCoin = new Coin
                {
                    Symbol = balance.Currency,
                    Balance = balance.Balance,
                    Price = $"BTC {GetCoinBtcValue(balance.Currency):0.########}"
                };

                if (newCoin.Symbol != "BTC")
                {
                    newCoin.BtcValue += 
                        balance.Balance * GetCoinBtcValue(balance.Currency);
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
                    _summaries
                        .Result
                        .FirstOrDefault(x => x.MarketName == $"USDT0BTC")?.Last ?? 0);
        }

        public decimal GetCoinBtcValue(string coin)
        {
            return _summaries
                       .Result
                       .FirstOrDefault(x => x.MarketName == $"BTC-{coin}")?.Last ?? 0;
        }
    }
}