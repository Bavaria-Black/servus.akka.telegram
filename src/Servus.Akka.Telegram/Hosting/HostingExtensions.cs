using Microsoft.Extensions.DependencyInjection;
using Servus.Akka.Telegram.Services;

namespace Servus.Akka.Telegram.Hosting;

public static class HostingExtensions
{
    public static IServiceCollection UseTelegramBotService(this IServiceCollection serviceCollection)
    {
        return serviceCollection.AddHostedService<MessageUpdateService>();
    }
}