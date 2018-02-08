using System;
using System.Timers;
using Venus.Infrastructure.Events;
using Venus.Infrastructure.Events.EventClasses;

namespace Venus.Infrastructure
{
    public class TimerManager
    {
        private double _interval = 30; // default interval in seconds

        private readonly Timer _timer;
        private readonly IEventPublisher _publisher;
        
        public TimerManager(IEventPublisher publisher)
        {
            _publisher = publisher;

            _publisher
                .GetEvent<UserSettingsUpdatedEvent>()
                .Subscribe(OnUserSettingsUpdate);

            _timer = new Timer(_interval * 1000);
            _timer.Elapsed += Update;
            _timer.Start();
        }

        public TimerManager(double interval)
        {
            _interval = interval;
            _timer = new Timer(interval * 1000);
            _timer.Elapsed += Update;
            _timer.Start();
        }

        public void OnUserSettingsUpdate(UserSettingsUpdatedEvent e)
        {

            if(e?.NewConfiguration == null) { return; }

            _interval = e.NewConfiguration.UpdateInterval;

            RestartTimer();
        }

        public void RestartTimer()
        {
            _timer.Stop();
            _timer.Interval = _interval * 1000;
            _timer.Start();
        }

        private void Update(object sender, EventArgs e)
        {
            _publisher.Publish(new TimerTickElapsedEvent());
        }
    }
}