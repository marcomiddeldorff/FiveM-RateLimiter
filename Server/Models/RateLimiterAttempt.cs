

namespace RateLimiter.Server.Models
{
    public class RateLimiterAttempt
    {
        public int CurrentAttempts { get; set; }

        public long LatestAttemptAt { get; set; }
    }
}