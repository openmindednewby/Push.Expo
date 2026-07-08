namespace Push.Expo.Abstractions;

/// <summary>Per-token delivery outcome from a push send.</summary>
public sealed class PushTokenResult
{
  /// <summary>The device push token this result refers to.</summary>
  public required string Token { get; init; }

  /// <summary>True if Expo accepted the message for this token.</summary>
  public required bool Ok { get; init; }

  /// <summary>Expo error code when not OK (e.g. <c>DeviceNotRegistered</c>, <c>InvalidCredentials</c>).</summary>
  public string? Error { get; init; }

  /// <summary>Expo ticket id when accepted (used to later query receipts).</summary>
  public string? TicketId { get; init; }
}

/// <summary>Aggregate result of a push send.</summary>
public sealed class PushSendResult
{
  /// <summary>True if at least one token was accepted and no transport-level error occurred.</summary>
  public required bool Success { get; init; }

  /// <summary>Per-token outcomes (same order as the request, including locally-rejected invalid tokens).</summary>
  public required IReadOnlyList<PushTokenResult> Results { get; init; }

  /// <summary>Transport-level error (e.g. HTTP failure) when the whole request failed.</summary>
  public string? Error { get; init; }

  /// <summary>Tokens Expo reported as <c>DeviceNotRegistered</c> — the caller should delete these.</summary>
  public IReadOnlyList<string> UnregisteredTokens =>
    Results.Where(r => r.Error == "DeviceNotRegistered").Select(r => r.Token).ToList();
}
