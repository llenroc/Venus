using Venus.Model;

namespace Venus.Infrastructure.Events.EventClasses
{
    public class UserSettingsUpdatedEvent
    {
        public object Sender { get; }
        public Configuration NewConfiguration { get; }

        public UserSettingsUpdatedEvent(object sender, Configuration newConfig)
        {
            Sender = sender;
            NewConfiguration = newConfig;
        }
    }
}