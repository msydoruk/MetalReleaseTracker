namespace MetalReleaseTracker.Core.Entities
{
    public class Subscription
    {
        public Guid Id { get; set; }

        public string Email { get; set; }

        public bool NotifyForNewReleases { get; set; }
    }
}
