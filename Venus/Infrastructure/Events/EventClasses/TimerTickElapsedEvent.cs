using System;

namespace Venus.Infrastructure.Events.EventClasses
{
    public class TimerTickElapsedEvent
    {
        public object Sender { get; }
        public EventArgs EventArgs { get; set; }

        public TimerTickElapsedEvent()
        {
            Sender = null;
            EventArgs = EventArgs.Empty;
        }
        public TimerTickElapsedEvent(object sender, EventArgs eventArgs)
        {
            Sender = sender;
            EventArgs = eventArgs;
        }
    }
}