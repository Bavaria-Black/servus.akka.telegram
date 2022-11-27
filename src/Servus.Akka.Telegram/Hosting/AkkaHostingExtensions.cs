using System.Security.Cryptography;
using Akka.Actor;
using Akka.Cluster.Hosting;
using Akka.DependencyInjection;
using Akka.Hosting;
using Akka.Remote.Hosting;
using Servus.Akka.Telegram.Messages;
using Servus.Akka.Telegram.Services;

namespace Servus.Akka.Telegram.Hosting;

public static class AkkaHostingExtensions
{
    public static IActorRef ResolveActor<T>(this ActorSystem system, string name) where T : ActorBase
        => system.ActorOf(DependencyResolver.For(system).Props<T>(), name);

    public static void TryRegisterDiActor<T>(this IActorRegistry registry, ActorSystem system, string name)
        where T : ActorBase
        => registry.TryRegister<T>(system.ResolveActor<T>(name));

    public static AkkaConfigurationBuilder AddTelegramCluster(this AkkaConfigurationBuilder builder,
        string hostName, int port, string[] seedNodes, params string[] additionalRoles)
    {
        var roles = new List<string>(additionalRoles) {"telegram-user-shard"};
        return builder
            .WithRemoting(hostName, port)
            .WithClustering(new ClusterOptions()
            {
                Roles = roles.ToArray(),
                SeedNodes = seedNodes.Select(Address.Parse).ToArray()
            }).WithActors((system, registry) =>
            {
                registry.TryRegisterDiActor<InvitationController>(system, "invitation-controller");
                registry.TryRegisterDiActor<TelegramIngress>(system, "telegram-ingress");
                registry.TryRegisterDiActor<TelegramEgress>(system, "telegram-egress");
            })
            .WithShardRegion<UserShardRegion>(
                typeName: "user-region",
                compositePropsFactory: (system, registry) =>
                {
                    return (e) => DependencyResolver.For(system).Props<UserShardRegion>(long.Parse(e));
                },
                new UserMessageExtractor(5),
                new ShardOptions() {Role = "telegram-user-shard"}
            );
    }

    public static AkkaConfigurationBuilder WithCommandWorker<T>(this AkkaConfigurationBuilder builder,
        string commandName, int paramCount = 0, bool joinParams = false, string requiredRole = "") where T : ActorBase
    {
        if (paramCount == 1 && commandName is "/start" or "start")
        {
            throw new ArgumentException("it is not possible to register the /start command with more than 0 params",
                nameof(paramCount));
        }

        return builder.WithActors((system, _) =>
        {
            var commandRegistry = CommandRegistry.For(system);
            commandRegistry.RegisterCommand(commandName, paramCount, joinParams, requiredRole,
                (user) => DependencyResolver.For(system).Props<T>(user));
        });
    }
}