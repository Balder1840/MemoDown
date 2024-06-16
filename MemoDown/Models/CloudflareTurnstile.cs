using System.Text.Json.Serialization;

namespace MemoDown.Models
{

    public record class CloudflareTurnstileVerifyRequestModel(
        // https://developers.cloudflare.com/turnstile/get-started/server-side-validation
        [property: JsonPropertyName("secret")] string SecretKey,
        [property: JsonPropertyName("response")] string Token,
        [property: JsonPropertyName("remoteip")] string? UserIpAddress,
        [property: JsonPropertyName("idempotency_key")] string? IdempotencyKey);

    public record class CloudflareTurnstileVerifyResult(
    // https://developers.cloudflare.com/turnstile/get-started/server-side-validation/
    [property: JsonPropertyName("success")] bool Success,
    [property: JsonPropertyName("error-codes")] string[] ErrorCodes,
    [property: JsonPropertyName("challenge_ts")] DateTimeOffset On,
    [property: JsonPropertyName("hostname")] string Hostname
    );
}
