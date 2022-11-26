using Servus.Akka.Telegram.Messages;
using Servus.Akka.Telegram.Users;

namespace Servus.Akka.Telegram;

internal class CommandRegistry
{
    private readonly string _adminRole;
    private readonly List<CommandRegistration> _commands = new();

    public CommandRegistry(string adminRole = "admin")
    {
        _adminRole = adminRole;
    }

    public void RegisterCommand(string commandName, int paramCount, bool joinParams, string requiredRole,
        Action<TelegramCommand, BotUser?> action)
    {
        var command = new CommandRegistration(commandName, paramCount, joinParams, requiredRole, _adminRole, action);
        _commands.Add(command);
    }

    public bool CheckAndExecute(TelegramCommand msg, BotUser? user)
        => _commands.Any(command => command.CheckAndRun(msg, user));
}