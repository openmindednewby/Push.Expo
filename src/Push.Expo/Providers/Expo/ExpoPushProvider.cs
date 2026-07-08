using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Push.Expo.Abstractions;

namespace Push.Expo.Providers.Expo;

/// <summary>
/// <see cref="IPushProvider"/> backed by the Expo push API (https://exp.host).
/// Locally rejects malformed tokens, sends valid ones in a single batch request, and maps
/// Expo's per-message tickets back to <see cref="PushTokenResult"/> (incl. DeviceNotRegistered).
/// </summary>
public sealed partial class ExpoPushProvider : IPushProvider
{
  private const string InvalidTokenError = "InvalidExpoPushToken";

  private readonly HttpClient _httpClient;
  private readonly ExpoPushOptions _options;
  private readonly ILogger<ExpoPushProvider> _logger;

  public ExpoPushProvider(
    HttpClient httpClient,
    IOptions<ExpoPushOptions> options,
    ILogger<ExpoPushProvider> logger)
  {
    _httpClient = httpClient;
    _options = options.Value;
    _logger = logger;
  }

  [GeneratedRegex(@"^Expo(nent)?PushToken\[.+\]$")]
  private static partial Regex ExpoTokenRegex();

  /// <summary>True if <paramref name="token"/> is a syntactically valid Expo push token.</summary>
  public static bool IsValidExpoPushToken(string? token) =>
    !string.IsNullOrWhiteSpace(token) && ExpoTokenRegex().IsMatch(token);

  /// <inheritdoc />
  public async Task<PushSendResult> SendAsync(PushMessage message, CancellationToken cancellationToken = default)
  {
    var invalid = message.Tokens.Where(t => !IsValidExpoPushToken(t)).ToList();
    var valid = message.Tokens.Where(IsValidExpoPushToken).ToList();

    var results = new List<PushTokenResult>(invalid.Select(t => new PushTokenResult
    {
      Token = t,
      Ok = false,
      Error = InvalidTokenError,
    }));

    if (valid.Count == 0)
    {
      return new PushSendResult { Success = false, Results = results };
    }

    var payload = valid.Select(token => new ExpoMessageDto
    {
      To = token,
      Title = message.Title,
      Body = message.Body,
      Data = message.Data,
      Sound = message.Sound,
      Badge = message.Badge,
      ChannelId = message.ChannelId,
    }).ToList();

    try
    {
      using var request = new HttpRequestMessage(HttpMethod.Post, _options.PushApiUrl)
      {
        Content = JsonContent.Create(payload),
      };
      if (!string.IsNullOrWhiteSpace(_options.AccessToken))
      {
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.AccessToken);
      }

      using var response = await _httpClient.SendAsync(request, cancellationToken);
      if (!response.IsSuccessStatusCode)
      {
        _logger.LogError("Expo push API returned {Status} for {Count} tokens", (int)response.StatusCode, valid.Count);
        results.AddRange(valid.Select(t => new PushTokenResult { Token = t, Ok = false, Error = $"Http{(int)response.StatusCode}" }));
        return new PushSendResult { Success = false, Results = results, Error = $"Expo push API HTTP {(int)response.StatusCode}" };
      }

      var body = await response.Content.ReadFromJsonAsync<ExpoResponseDto>(cancellationToken);
      var tickets = body?.Data ?? new List<ExpoTicketDto>();

      for (var i = 0; i < valid.Count; i++)
      {
        var ticket = i < tickets.Count ? tickets[i] : null;
        var ok = ticket?.Status == "ok";
        results.Add(new PushTokenResult
        {
          Token = valid[i],
          Ok = ok,
          TicketId = ok ? ticket?.Id : null,
          Error = ok ? null : ticket?.Details?.Error ?? ticket?.Message ?? "UnknownExpoError",
        });
      }

      var anyOk = results.Any(r => r.Ok);
      return new PushSendResult { Success = anyOk, Results = results };
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to send Expo push to {Count} tokens", valid.Count);
      results.AddRange(valid.Select(t => new PushTokenResult { Token = t, Ok = false, Error = "TransportError" }));
      return new PushSendResult { Success = false, Results = results, Error = ex.Message };
    }
  }

  // ---- Expo wire DTOs ----

  private sealed class ExpoMessageDto
  {
    [JsonPropertyName("to")] public required string To { get; init; }
    [JsonPropertyName("title")] public required string Title { get; init; }
    [JsonPropertyName("body")] public required string Body { get; init; }
    [JsonPropertyName("data")] public IReadOnlyDictionary<string, string>? Data { get; init; }
    [JsonPropertyName("sound")] public string? Sound { get; init; }
    [JsonPropertyName("badge")] public int? Badge { get; init; }
    [JsonPropertyName("channelId")] public string? ChannelId { get; init; }
  }

  private sealed class ExpoResponseDto
  {
    [JsonPropertyName("data")] public List<ExpoTicketDto>? Data { get; init; }
  }

  private sealed class ExpoTicketDto
  {
    [JsonPropertyName("status")] public string? Status { get; init; }
    [JsonPropertyName("id")] public string? Id { get; init; }
    [JsonPropertyName("message")] public string? Message { get; init; }
    [JsonPropertyName("details")] public ExpoTicketDetailsDto? Details { get; init; }
  }

  private sealed class ExpoTicketDetailsDto
  {
    [JsonPropertyName("error")] public string? Error { get; init; }
  }
}
