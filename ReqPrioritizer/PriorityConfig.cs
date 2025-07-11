namespace ReqPrioritizer
{
    public class PriorityConfig
    {
        public required string Name { get; set; }

        public bool UseQueue { get; set; } = false;

        public int MaxConcurrentRequests { get; set; } = 0;

        public int RateLimitPerSecond { get; set; } = 0;

        public int MaxQueueLength { get; set; } = 0;

        public int QueueWaitTimeoutMs { get; set; } = 0;

        public LimitExceededAction OnLimitExceeded { get; set; } = LimitExceededAction.Reject;
    }

    public enum LimitExceededAction
    {
        Reject,
        Queue,
        Wait
    }

    public class PriorityOptions
    {
        public Dictionary<string, PriorityConfig> Priorities { get; set; } = new();
    }
}
