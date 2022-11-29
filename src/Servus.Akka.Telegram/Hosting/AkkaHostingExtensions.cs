using System.Security.Cryptography;
using Akka.Actor;
using Akka.Cluster.Hosting;
using Akka.DependencyInjection;
using Akka.Hosting;
using Akka.Remote.Hosting;
using Servus.Akka.Telegram.Messages;
using Servus.Akka.Telegram.Registry;
using Servus.Akka.Telegram.Services;
using Servus.Akka.Telegram.Users;

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

    public static AkkaConfigurationBuilder WithCommandWorker<T>(this AkkaConfigurationBuilder configurationBuilder,
        string requiredRole, Action<CommandListBuilder> builder) where T : ActorBase
    {
        return configurationBuilder.WithActors((system, _) =>
        {
            Props PropsFactory(BotUser user) => DependencyResolver.For(system).Props<T>(user);

            var workerRegistry = WorkerRegistry.For(system);
            var commandListBuilder = new CommandListBuilder(requiredRole, PropsFactory);
            
            builder(commandListBuilder);
            
            workerRegistry.Register(commandListBuilder.Build());
        });
    }
}