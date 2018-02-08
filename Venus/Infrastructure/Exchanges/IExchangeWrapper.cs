using System.Collections.Generic;
using System.Threading.Tasks;
using Venus.Infrastructure.Exchanges;
using Venus.Model;

namespace Venus.Infrastructure
{
    public interface IExchangeWrapper
    {
        SupportedExchanges Exchange { get; }
        bool IsConfigured { get; }
        List<Coin> GetAccountData();
        
        void Init(ApiKeyData keyData);
        void UpdateKeyData(ApiKeyData keyData);
        void UpdateMarketSummary();
        Task<decimal> GetBitcoinPrice();
        decimal GetCoinBtcValue(string coin);
    }
}