namespace Venus.Infrastructure.Events.EventClasses
{
    public class PortfolioUpdatedEvent
    {
        public Portfolio Portfolio { get; }

        public PortfolioUpdatedEvent(Portfolio portfolio)
        {
            Portfolio = portfolio;
        }
    }
}