# Push.Expo

Expo push-notification provider for .NET. Sends mobile push via the [Expo push API](https://docs.expo.dev/push-notifications/sending-notifications/)
(`exp.host`) with per-token delivery results and `DeviceNotRegistered` detection — so the caller can
prune dead tokens.

## Install

```bash
dotnet add package Push.Expo
```

## Register

```csharp
// Options via lambda
builder.Services.AddExpoPush(o => o.AccessToken = builder.Configuration["ExpoPush:AccessToken"]);

// or bind the "ExpoPush" config section
builder.Services.AddExpoPush(builder.Configuration);
```

## Send

```csharp
public sealed class MyService(IPushProvider push)
{
  public async Task NotifyAsync(IReadOnlyList<string> tokens)
  {
    var result = await push.SendAsync(new PushMessage
    {
      Tokens = tokens,
      Title = "New message",
      Body = "You have a new notification",
      Data = new Dictionary<string, string> { ["screen"] = "inbox" },
    });

    // Prune tokens Expo reported as gone:
    foreach (var dead in result.UnregisteredTokens) { /* delete from your store */ }
  }
}
```

- `SendAsync` never throws for delivery failures — transport + per-token errors are on `PushSendResult`.
- Malformed tokens (not `ExponentPushToken[...]` / `ExpoPushToken[...]`) are rejected locally without a network call.
- `PushSendResult.UnregisteredTokens` lists tokens Expo reported as `DeviceNotRegistered`.

## Options

| Option | Default | Notes |
|--------|---------|-------|
| `PushApiUrl` | `https://exp.host/--/api/v2/push/send` | Expo push endpoint. |
| `AccessToken` | _(none)_ | Optional Expo access token (sent as Bearer); recommended for production. |

## License

MIT
