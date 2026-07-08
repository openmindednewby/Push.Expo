namespace Push.Expo.Abstractions;

/// <summary>Sends mobile push notifications to device tokens.</summary>
public interface IPushProvider
{
  /// <summary>
  /// Sends a push message to its target tokens. Never throws for delivery failures —
  /// transport/per-token errors are reported on the returned <see cref="PushSendResult"/>.
  /// </summary>
  Task<PushSendResult> SendAsync(PushMessage message, CancellationToken cancellationToken = default);
}
