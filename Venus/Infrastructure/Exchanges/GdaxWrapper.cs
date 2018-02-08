using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using GDAXClient.Authentication;
using GDAXClient.Services.Accounts.Models;
using GDAXClient.Services.Products.Models;
using GDAXClient.Shared;
using Newtonsoft.Json;
using Venus.Model;

namespace Venus.Infrastructure.Exchanges
{
    public class GdaxWrapper : IExchangeWrapper
    {
        public SupportedExchanges Exchange { get; }
        public bool IsConfigured { get; private set; }
        private ApiKeyData _apiKey;
        private Authenticator _authenticator;
        private GDAXClient.GDAXClient _gdaxClient;
        private List<Product> _summary; 

        public GdaxWrapper()
        {
            Exchange = SupportedExchanges.GDax;
            _gdaxClient = new GDAXClient.GDAXClient(new Authenticator("", "", ""));
        }

        public void Init(ApiKeyData keyData)
        {
            if (string.IsNullOrWhiteSpace(keyData.GetRawApiKey()))
            {
                IsConfigured = false;
            }

            UpdateKeyData(keyData);
            UpdateMarketSummary();
        }

        public void UpdateKeyData(ApiKeyData keyData)
        {
            _apiKey = keyData;
            _authenticator = new Authenticator(
                _apiKey.GetRawApiKey(), 
                _apiKey.GetRawApiSecret(),
                _apiKey.GetRawApiPassword());
            _gdaxClient = new GDAXClient.GDAXClient(_authenticator);
            IsConfigured = true;
        }

        public void UpdateMarketSummary()
        {
            if (_apiKey == null || !IsConfigured) { return; }

            try
            {
                _summary = Task.Run(
                    () => _gdaxClient
                        .ProductsService
                        .GetAllProductsAsync()).Result.ToList();
            }
            catch (Exception e)
            {
                // TODO: Handle Gdax GetAllProductsAsync() exception
            }
        }

        public List<Coin> GetAccountData()
        {
            IEnumerable<Account> allAccounts = new List<Account>();

            try
            {
                allAccounts = Task.Run(
                    () => _gdaxClient.AccountsService.GetAllAccountsAsync()).Result;
            }
            catch (Exception e)
            {
                // TODO: Handle Gdax GetAllAcountsAsyc() exceptions
            }
            
            
            var coinList = new List<Coin>();
            if (!allAccounts.Any()) { return coinList; }

            foreach (var acct in allAccounts)
            {
                if(acct.Balance == 0) { continue;}

                var newCoin = new Coin
                {
                    Symbol = acct.Currency,
                    Balance = acct.Balance,
                    Price = $"USD ${GetValueByPair(acct.Currency, "USD"):0.##}"
                };

                if (newCoin.Symbol == "BTC")
                {
                    newCoin.BtcValue = acct.Balance;
                }
                else
                {
                    newCoin.BtcValue = 
                        GetCoinBtcValue(newCoin.Symbol) * newCoin.Balance;
                }

                coinList.Add(newCoin);
            }

            return coinList;
        }

        public Task<decimal> GetBitcoinPrice()
        {
            return Task.Run( () => _gdaxClient?
                                       .ProductsService
                                       .GetProductTickerAsync(ProductType.BtcUsd)
                                       .Result.Price ?? 0);
        }

        public decimal GetCoinBtcValue(string coin)
        {
            if (coin == "USD") { return 0; }

            return GetValueByPair(coin, "BTC");
        }

        private decimal GetValueByPair(string coin, string tradedAgainst)
        {
            var request = WebRequest
                .CreateHttp(
                    $"https://api.gdax.com/products/{coin}-{tradedAgainst}/ticker");
            request.Credentials = CredentialCache.DefaultCredentials;
            request.UserAgent = ".NET Framework Client";

            WebResponse response = null;

            // Get the response.  
            try
            {
                response = request.GetResponse();
            }
            catch
            {
                return 0;
            }

            // Get the stream containing content returned by the server.  
            var dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.  
            var reader = new StreamReader(dataStream);
            // Read the content.  
            var responseFromServer = reader.ReadToEnd();

            var tickerobj = JsonConvert
                .DeserializeObject<TickerResponse>(responseFromServer);

            reader.Close();
            response.Close();

            return decimal.Parse(tickerobj.Price);
        }
    }
}