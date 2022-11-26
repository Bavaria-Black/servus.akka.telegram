using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Servus.Akka.Telegram.Services;

namespace Servus.Akka.Telegram.Hosting;

public static class HostingExtensions
{
    public static IServiceCollection UseTelegramBotService(this IServiceCollection serviceCollection)
    {
        return serviceCollection.AddHostedService<MessageUpdateService>();
    }
    
    public static T GetConfiguration<T>(this IServiceProvider sp) where T : class, new()
        => sp.GetRequiredService<IOptions<T>>().Value;
}