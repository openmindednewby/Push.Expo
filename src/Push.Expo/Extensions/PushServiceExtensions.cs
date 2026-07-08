using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Push.Expo.Abstractions;
using Push.Expo.Providers.Expo;

namespace Push.Expo.Extensions;

/// <summary>DI registration for the Expo push provider.</summary>
public static class PushServiceExtensions
{
  /// <summary>
  /// Registers <see cref="IPushProvider"/> (Expo) with a typed <see cref="HttpClient"/>.
  /// </summary>
  public static IServiceCollection AddExpoPush(
    this IServiceCollection services,
    Action<ExpoPushOptions>? configureOptions = null)
  {
    if (configureOptions != null)
    {
      services.Configure(configureOptions);
    }

    services.AddHttpClient<IPushProvider, ExpoPushProvider>();
    return services;
  }

  /// <summary>
  /// Registers <see cref="IPushProvider"/> (Expo), binding options from the
  /// <c>ExpoPush</c> configuration section.
  /// </summary>
  public static IServiceCollection AddExpoPush(
    this IServiceCollection services,
    IConfiguration configuration)
  {
    services.Configure<ExpoPushOptions>(configuration.GetSection(ExpoPushOptions.SectionName));
    services.AddHttpClient<IPushProvider, ExpoPushProvider>();
    return services;
  }
}
