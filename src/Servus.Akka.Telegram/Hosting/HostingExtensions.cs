using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Servus.Akka.Telegram.Hosting.Configuration;
using Servus.Akka.Telegram.Services;

namespace Servus.Akka.Telegram.Hosting;

public static class HostingExtensions
{
    public static IServiceCollection UseTelegramBotService(this IServiceCollection serviceCollection, HostBuilderContext context)
    {
        // Register Bot configuration
        serviceCollection.Configure<BotConfiguration>(
            context.Configuration.GetSection(BotConfiguration.SectionName));
        // Register Bot configuration
        serviceCollection.Configure<UserRegistrationConfiguration>(
            context.Configuration.GetSection(UserRegistrationConfiguration.SectionName));
        
        // TelegramBotAPI polling service
        return serviceCollection.AddHostedService<MessageUpdateService>();
    }
    
    public static T GetConfiguration<T>(this IServiceProvider sp) where T : class, new()
        => sp.GetRequiredService<IOptions<T>>().Value;
}