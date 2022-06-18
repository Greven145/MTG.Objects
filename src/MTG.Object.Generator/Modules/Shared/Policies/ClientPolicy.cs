using Polly;
using Polly.Retry;

namespace MTG.Object.Generator.Modules.Shared.Policies;

internal class ClientPolicy {
    public const int RetryAttempts = 5;

    public AsyncRetryPolicy<HttpResponseMessage> ExponentialBackOff { get; } =
        Policy
            .HandleResult<HttpResponseMessage>(res => !res.IsSuccessStatusCode)
            .WaitAndRetryAsync(RetryAttempts, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));
}