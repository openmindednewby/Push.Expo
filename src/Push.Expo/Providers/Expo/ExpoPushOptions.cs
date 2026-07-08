namespace Push.Expo.Providers.Expo;

/// <summary>Configuration for the Expo push provider.</summary>
public sealed class ExpoPushOptions
{
  /// <summary>Config section name for binding.</summary>
  public const string SectionName = "ExpoPush";

  /// <summary>The Expo push API endpoint.</summary>
  public string PushApiUrl { get; set; } = "https://exp.host/--/api/v2/push/send";

  /// <summary>
  /// Optional Expo access token. When set, sent as a Bearer token to enforce that only
  /// holders of the token can send to your project (recommended for production).
  /// </summary>
  public string? AccessToken { get; set; }
}
