using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using RateLimiter.Server.Models;

namespace RateLimiter.Server
{
    public class ServerMain : BaseScript
    {
        /// <summary>
        /// All registered rate limiter.
        /// </summary>
        public IDictionary<string, RegisteredRateLimiter> RegisteredRateLimiters;

        public delegate bool IsPlayerInTimeoutDelegate(string identifier);

        public ServerMain()
        {
            RegisteredRateLimiters = new Dictionary<string, RegisteredRateLimiter>();

            EventHandlers.Add("playerDropped", new Action<Player, string>(OnPlayerDropped));

            IsPlayerInTimeoutDelegate getIsPlayerInTimoutDelegate = new IsPlayerInTimeoutDelegate(IsPlayerInTimeout);

            #region Exports (Server)
            Exports.Add("IsPlayerInTimeout", getIsPlayerInTimoutDelegate);
            Exports.Add("RegisterRateLimiter", new Action<string, int>(RegisterRateLimiter));
            Exports.Add("HitRateLimiter", new Action<string, string>(HitRateLimiter));
            #endregion
        }

        /// <summary>
        /// Registers a new rate limiter.
        /// </summary>
        /// <param name="Name">A unique name for the rate limiter.</param>
        /// <param name="AttemptsPerMinute">The attempts a user can make per minute.</param>
        public void RegisterRateLimiter(string Name, int AttemptsPerMinute)
        {
            if (RegisteredRateLimiters.ContainsKey(Name))
            {
                Debug.WriteLine($"[^3WARNING^0] The rate limiter with name {Name} has already been registered. Make sure to use unique names for your rate limiter.");
                return;
            }

            // Create a new instance of the rate limiter and add it to our list of registerd rate limiters.
            RegisteredRateLimiter RateLimiter = new() 
            {
                Name = Name,
                AttemptsPerMinute = AttemptsPerMinute,
                Attempts = new Dictionary<string, RateLimiterAttempt>()
            };

            RegisteredRateLimiters.Add(Name, RateLimiter);

            Debug.WriteLine($"[^4INFO^0] Ratelimiter with name {Name} has been successfully registered. Allowed attempts per minute: {AttemptsPerMinute}");
        }

        public void HitRateLimiter(string Identifier, string Name)
        {
            // Check if the rate limiter has been registered already.
            if (!RegisteredRateLimiters.ContainsKey(Name))
            {
                Debug.WriteLine($"[^3WARNING^0] The rate limiter with given name {Name} does not exist. Please check your code and make sure to register the rate limiter.");
                return;
            }

            // Try to get the rate limiter object within our dictionary.
            RegisteredRateLimiters.TryGetValue(Name, out RegisteredRateLimiter RateLimiter);

            bool PlayerExists = RateLimiter.Attempts.ContainsKey(Identifier);

            // We are going to add the player if he has not been added to the current rate limiter hits yet.
            if (!PlayerExists)
            {
                RateLimiter.Attempts.Add(Identifier, new RateLimiterAttempt() { CurrentAttempts = 1, LatestAttemptAt = API.GetGameTimer() });
                return;
            }

            // Try to get the value so we can increase the current attempt.
            RateLimiter.Attempts.TryGetValue(Identifier, out RateLimiterAttempt Attempt);

            // Increase the attempt by one.
            Attempt.CurrentAttempts++;
            // Set the latest attempt to the current game timer.
            Attempt.LatestAttemptAt = API.GetGameTimer();

            // Override it in our dictionary.
            RateLimiter.Attempts[Identifier] = Attempt;
        }

        public bool IsPlayerInTimeout(string Identifier, string Name)
        {
            if (!RegisteredRateLimiters.ContainsKey(Name))
            {
                Debug.WriteLine($"[^1ERROR^0] The rate limiter name \"{Name}\" is not a valid rate limiter name.");
                return false;
            }

            var RateLimiter = RegisteredRateLimiters[Name];

            return RateLimiter.Attempts[Identifier].CurrentAttempts < RateLimiter.AttemptsPerMinute;
        }

        public bool IsPlayerInTimeout(string Identifier)
        {
            // Loop through all registered rate limiters.
            foreach (var RateLimiterKey in RegisteredRateLimiters.Keys)
            {
                var RateLimiter = RegisteredRateLimiters[RateLimiterKey];
                // Check if the rate limiter contains the given identifier.
                if (RateLimiter.Attempts.ContainsKey(Identifier))
                {
                    var Attempt = RateLimiter.Attempts[Identifier];

                    // Check if the current attempts the player made exceed the maximum allowed attempts per minute.
                    if (Attempt.CurrentAttempts >= RateLimiter.AttemptsPerMinute)
                    {
                        // The player made more attempts than allowed.
                        return true;
                    }
                }
            }

            // The player is not in a current timeout.
            return false;
        }

        public void OnPlayerDropped([FromSource] Player Player, string Reason)
        {
            // Get the identifier of the player.
            string identifier = GetPlayerLicenseIdentifier(Player.Handle);

            // Loop through all registered rate limiters.
            foreach (var RateLimiterKey in RegisteredRateLimiters.Keys.ToList())
            {
                var RateLimiter = RegisteredRateLimiters[RateLimiterKey];

                // Check if the attempts contain the identifier.
                if (RateLimiter.Attempts.ContainsKey(identifier))
                {
                    // Remove the identifier out of the list of attempts.
                    RegisteredRateLimiters[RateLimiterKey].Attempts.Remove(identifier);
                }
            }
        }

        [Tick]
        public async Task OnTick()
        {
            // Loop through all registered rate limiters.
            foreach (var RateLimiterKey in RegisteredRateLimiters.Keys.ToList())
            {
                var RateLimiter = RegisteredRateLimiters[RateLimiterKey];

                // Loop through all players which have made at least one interaction with one rate limiter.
                foreach (var PlayerIdentifier in RateLimiter.Attempts.Keys.ToList())
                {
                    var Attempt = RateLimiter.Attempts[PlayerIdentifier];

                    // If the latest attempt plus one minute is less than the current game timer the user will be able to make the same action again.
                    if (API.GetGameTimer() > (Attempt.LatestAttemptAt + 60000))
                    {
                        // Reset the current attempts of the player.
                        Attempt.CurrentAttempts = 0;

                        RateLimiter.Attempts[PlayerIdentifier] = Attempt;
                    }
                }
            }

            await Delay(1000);
        }
        
        /// <summary>
        /// Internal function to return the player identifier.
        /// </summary>
        /// <param name="Player">The player source.</param>
        /// <returns>The players license identifier without the "license:" prefix.</returns>
        private string GetPlayerLicenseIdentifier(string source)
        {
            var license = API.GetPlayerIdentifierByType(source, "license");

            return license.Substring(8);
        }
    }
}