namespace Push.Expo.Abstractions;

/// <summary>
/// A push notification to deliver to one or more device push tokens.
/// </summary>
public sealed class PushMessage
{
  /// <summary>The Expo push tokens to deliver to (e.g. <c>ExponentPushToken[...]</c>).</summary>
  public required IReadOnlyList<string> Tokens { get; init; }

  /// <summary>Notification title.</summary>
  public required string Title { get; init; }

  /// <summary>Notification body.</summary>
  public required string Body { get; init; }

  /// <summary>Optional structured data payload delivered with the notification.</summary>
  public IReadOnlyDictionary<string, string>? Data { get; init; }

  /// <summary>Sound to play (default: "default"). Set to null for a silent notification.</summary>
  public string? Sound { get; init; } = "default";

  /// <summary>Optional iOS badge count.</summary>
  public int? Badge { get; init; }

  /// <summary>Optional Android notification channel id.</summary>
  public string? ChannelId { get; init; }
}
