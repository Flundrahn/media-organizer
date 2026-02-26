using System.Net;

namespace MLModel_TrainingDataApp.Common;

/// <summary>
/// HTTP retry policy with exponential backoff for handling rate limiting.
/// </summary>
public static class HttpRetryPolicy
{
    public static async Task<T> ExecuteAsync<T>(
        Func<Task<T>> action,
        int baseDelayMs = 300,
        int maxRetries = 6)
        where T : class?
    {
        var rnd = new Random();
        int tries = 0;

        while (true)
        {
            try
            {
                return await action();
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
            {
                tries++;
                if (tries > maxRetries) throw;

                int backoff = baseDelayMs * (int)Math.Pow(2, tries);
                var jitter = (int)(backoff * (0.2 * (rnd.NextDouble() - 0.5)));
                var wait = Math.Max(200, backoff + jitter);

                Console.WriteLine($"  Rate limited — retry {tries}/{maxRetries} after {wait}ms...");
                await Task.Delay(wait);
            }
            catch (HttpRequestException)
            {
                throw;
            }
        }
    }
}
