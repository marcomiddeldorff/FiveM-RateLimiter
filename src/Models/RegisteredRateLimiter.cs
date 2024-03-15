

using System.Collections.Generic;

namespace RateLimiter.Server.Models
{
    public class RegisteredRateLimiter
    {
        public string Name { get; set; }

        public int AttemptsPerMinute { get; set; }

        public IDictionary<string, RateLimiterAttempt> Attempts { get; set; }
    }
}