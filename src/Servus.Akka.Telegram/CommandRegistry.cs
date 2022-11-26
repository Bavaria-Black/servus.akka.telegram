using Akka.Actor;
using Servus.Akka.Telegram.Hosting;
using Servus.Akka.Telegram.Messages;
using Servus.Akka.Telegram.Users;

namespace Servus.Akka.Telegram;

public class CommandRegistry : IExtension
{
    private readonly string _adminRole;
    private readonly List<CommandRegistration> _commands = new();

    public CommandRegistry(string adminRole = "admin")
    {
        _adminRole = adminRole;
    }

    public void RegisterCommand(string commandName, int paramCount, bool joinParams, string requiredRole, Props props)
    {
        var command = new CommandRegistration(commandName, paramCount, joinParams, requiredRole, _adminRole, props);
        _commands.Add(command);
    }

    internal bool CheckAndExecute(TelegramCommand msg, BotUser? user, Action<Props, string> action)
        => _commands.Any(command => command.CheckAndRun(msg, user, action));
    
    public static CommandRegistry For(ActorSystem actorSystem)
    {
        return actorSystem.WithExtension<CommandRegistry, CommandRegistryExtension>();
    }
}