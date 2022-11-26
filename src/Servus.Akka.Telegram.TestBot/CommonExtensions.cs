using Akka.Actor;
using Akka.DependencyInjection;
using Akka.Hosting;

namespace Servus.Akka.Telegram.TestBot;

public static class CommonExtensions
{
    public static IActorRef ResolveActor<T>(this ActorSystem system, string name) where T : ActorBase
        => system.ActorOf(DependencyResolver.For(system).Props<T>(), name);

    public static Props ResolveProps<T>(this ActorSystem system) where T : ActorBase
        => DependencyResolver.For(system).Props<T>();

    public static void TryRegisterDiActor<T>(this IActorRegistry registry, ActorSystem system, string name)
        where T : ActorBase
        => registry.TryRegister<T>(system.ResolveActor<T>(name));
}