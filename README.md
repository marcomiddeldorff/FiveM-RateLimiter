# Rate Limiter

This script adds a rate limiting functionality to your FiveM server. With this script, you'll be able to track if any of your players made too many requests to your server and put your players into a timeout if they exceed a configurable amount of requests per minute.

> ⚠️ **_NOTE:_** You will need some experience in FiveM development in order to use this script. I am happy to help you with implementing this script into your scripts but please make sure you have somewhat at least a small amount of knowledge.

## Usage

### Register a new rate limiter

> ⚠️ **_NOTE_**: The argument "name" has to be a **string**. The second argument "attemptsPerMinute" has to be a **integer**.

```lua
exports["oaky_ratelimiter"]:RegisterRateLimiter(rateLimiterName, attemptsPerMinute)
```

```csharp
Exports["oaky_ratelimiter"].RegisterRateLimiter(rateLimiterName, attemptsPerMinute)
```

### Hit the rate limiter

The rate limiter won't work if you don't trigger a hit of the rate limiter.
Please add this before each of your action and provide the correct name of your rate limiter.

> ⚠️ **_NOTE_**: The argument "name" and "identifier" must be a **string**.

```lua
exports["oaky_ratelimiter"]:HitRateLimiter(identifier, rateLimiterName)
```

```csharp
Exports["oaky_ratelimiter"].HitRateLimiter(identifier, rateLimiterName)
```

### Check if an player is in timeout

The return value of this export will only be True if a player has exceeded (MaxAttempts <= CurrentAttempts) the max allowed attempts which you specified in the `RegisterRateLimiter` export.

> ⚠️ **_NOTE_**: If you don't want to specify the name of a rate limiter please only provide the first argument. Don't provide null or anything else as the second argument.

```lua
local isInTimeout = exports["oaky_ratelimiter"]:IsPlayerInTimeout(identifier, rateLimiterName)
OR
local isInTimeout = exports["oaky_ratelimiter"]:IsPlayerInTimeout(identifier)
```

```csharp
bool IsInTimeout = Exports["oaky_ratelimiter"].IsPlayerInTimeout(identifier, rateLimiterName)
OR
bool IsInTimeout = Exports["oaky_ratelimiter"].IsPlayerInTimeout(identifier)
```

## Support
You can always reach out to me if you need help or have any questions. I am always happy to help.

You can also contact me on discord if you want: **oakyy**