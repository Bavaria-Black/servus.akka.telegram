using Akka.Actor;
using Akka.DependencyInjection;
using Akka.Hosting;

namespace Servus.Akka.Telegram.Hosting;

public static class AkkaHostingExtensions
{
    public static AkkaConfigurationBuilder WithCommandWorker<T>(this AkkaConfigurationBuilder builder, string commandName, int paramCount = 0, bool joinParams = false, string requiredRole = "") where T : ActorBase
    {
        return builder.WithActors((system, registry) =>
        {
            var commandRegistry = CommandRegistry.For(system);
            commandRegistry.RegisterCommand(commandName, paramCount, joinParams, requiredRole, DependencyResolver.For(system).Props<T>());
        });
    }
}