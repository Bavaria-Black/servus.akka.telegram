using Akka.Actor;
using Akka.Util.Internal;
using Servus.Akka.Telegram.Messages;
using Servus.Akka.Telegram.Users;

namespace Servus.Akka.Telegram;

internal class CommandRegistration
{
    private readonly string _commandName;
    private readonly int _paramCount;
    private readonly bool _joinParams;
    private readonly string _requiredRole;
    private readonly string _adminRole;
    private readonly Props _props;

    public CommandRegistration(string commandName, int paramCount, bool joinParams, string requiredRole,
        string adminRole, Props props)
    {
        if (!commandName.StartsWith("/"))
        {
            
            commandName = $"/{commandName}";
        }
        
        _commandName = commandName;
        _paramCount = paramCount;
        _joinParams = joinParams;
        _requiredRole = requiredRole;
        _adminRole = adminRole;
        _props = props;
    }

    internal bool CheckAndRun(TelegramCommand command, BotUser? user, Action<Props, string> action)
    {
        if (_joinParams && _paramCount == 1)
            command = command with
            {
                Parameters = new[]
                {
                    command.Parameters.Join(" ")
                }
            };

        if (user is null && _requiredRole != string.Empty)
            return false;
        
        if ( _requiredRole != string.Empty && !user!.Roles.Contains(_requiredRole) && !user.Roles.Contains(_adminRole)
             || command.Command != _commandName
             || command.Parameters.Count != _paramCount)
            return false;

        action(_props, _commandName[1..]);
        return true;
    }
}