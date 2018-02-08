namespace Venus.Infrastructure.Events.EventClasses
{
    public class RefreshStatusChangeEvent
    {
        public bool RefreshCurrentlyActive { get; }

        public RefreshStatusChangeEvent(bool refreshCurrentlyActive)
        {
            RefreshCurrentlyActive = refreshCurrentlyActive;
        }
    }
}