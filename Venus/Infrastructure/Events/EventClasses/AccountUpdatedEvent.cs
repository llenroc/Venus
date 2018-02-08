using System.Collections.Generic;
using Venus.Model;

namespace Venus.Infrastructure.Events.EventClasses
{
    public class AccountUpdatedEvent
    {
        public List<Coin> UpdatedCoinList { get; set; }
    }
}