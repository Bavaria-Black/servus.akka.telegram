using Akka.Actor;
using Servus.Akka.Telegram.Users;

namespace Servus.Akka.Telegram.Registry;

public class CommandListBuilder
{
    private readonly string _requiredRole;
    private readonly Func<BotUser, Props> _propsFactory;
    private readonly List<ExecutionRule> _executionRules = new();

    internal CommandListBuilder(string requiredRole, Func<BotUser, Props> propsFactory)
    {
        _requiredRole = requiredRole;
        _propsFactory = propsFactory;
    }

    public CommandListBuilder AddCommand(string commandName, int paramCount = 0, bool joinParams = false,
        string additionRole = "")
    {
        _executionRules.Add(new ExecutionRule(commandName, _requiredRole, paramCount, joinParams, additionRole));
        return this;
    }

    internal WorkerRegistration Build()
    {
        return new WorkerRegistration(_requiredRole, _propsFactory, _executionRules.ToArray());
    }
}