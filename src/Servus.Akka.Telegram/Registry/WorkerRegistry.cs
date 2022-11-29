using Akka.Actor;
using Servus.Akka.Telegram.Hosting;
using Servus.Akka.Telegram.Messages;
using Servus.Akka.Telegram.Users;

namespace Servus.Akka.Telegram.Registry;

public class WorkerRegistry : IExtension
{
    private readonly List<WorkerRegistration> _registrations = new();

    internal void Register(WorkerRegistration registration)
        => _registrations.Add(registration);

    internal bool CheckAndExecute(TelegramCommand msg, BotUser user, Action<Func<BotUser, Props>, string> action)
    {
        var executed = false;
        foreach (var workerRegistration in _registrations.Where(r => r.CanExecute(msg, user)))
        {
            action(workerRegistration.PropsFactory, workerRegistration.Id);
            executed = true;
        }

        return executed;
    }

    public static WorkerRegistry For(ActorSystem actorSystem)
    {
        return actorSystem.WithExtension<WorkerRegistry, WorkerRegistryExtension>();
    }
}

internal class WorkerRegistration
{
    internal string Id { get; } = Guid.NewGuid().ToString();
    internal Func<BotUser, Props> PropsFactory { get; }
    
    private readonly string _requiredRole;    
    private readonly ExecutionRule[] _executionRules;

    internal WorkerRegistration(string requiredRole, Func<BotUser, Props> propsFactory, ExecutionRule[] executionRules)
    {
        _requiredRole = requiredRole;
        PropsFactory = propsFactory;
        _executionRules = executionRules;
    }

    internal bool CanExecute(TelegramCommand command, BotUser user)
        => _executionRules.Any(r => r.CanExecute(command, user));
    
    
}