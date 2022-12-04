using System.ComponentModel.Design;
using Akka.Actor;
using LanguageExt;
using Servus.Akka.Telegram.Hosting;
using Servus.Akka.Telegram.Messages;
using Servus.Akka.Telegram.Users;

namespace Servus.Akka.Telegram.Registry;

public class WorkerRegistry : IExtension
{
    private readonly List<WorkerRegistration> _registrations = new();

    internal void Register(WorkerRegistration registration)
        => _registrations.Add(registration);

    internal bool CheckAndExecute(TelegramCommand msg, BotUser user,
        Action<Func<BotUser, Props>, string, CommandMessageBase> action)
    {
        var executed = false;
        foreach (var workerRegistration in _registrations.Where(r => r.CanExecute(msg, user)))
        {
            workerRegistration.GetExecutionRule(msg, user).Some(rule =>
            {
                CommandMessageBase message = rule.AllParametersSupplied(msg)
                    ? new CommandMessage(msg.Command, msg.ChatInformation, msg.Parameters)
                    : new IncompleteCommandMessage(msg.Command, msg.ChatInformation, msg.Parameters);
                
                action(workerRegistration.PropsFactory, workerRegistration.Id, message);
                executed = true;
            }).None(() => { });
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

    internal bool CanExecute(TelegramCommand command, BotUser user) =>
        _executionRules.Any(r => r.CanExecute(command, user));

    internal Option<ExecutionRule> GetExecutionRule(TelegramCommand command, BotUser user)
        => _executionRules.FirstOrDefault(r => r.CommandMatches(command) && r.RoleMatches(user));
}