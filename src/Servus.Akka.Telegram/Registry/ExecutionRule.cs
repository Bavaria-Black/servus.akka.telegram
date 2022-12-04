using Akka.Util.Internal;
using Servus.Akka.Telegram.Messages;
using Servus.Akka.Telegram.Users;

namespace Servus.Akka.Telegram.Registry;

internal class ExecutionRule
{
    private readonly string _commandName;
    private readonly int _paramCount;
    private readonly bool _joinParams;
    private readonly string _requiredRole;
    private readonly string _additionRole;

    public ExecutionRule(string commandName, string requiredRole, int paramCount = 0, bool joinParams = false,
        string additionRole = "")
    {
        if (!commandName.StartsWith("/"))
        {
            commandName = $"/{commandName}";
        }

        _commandName = commandName;
        _paramCount = paramCount;
        _joinParams = joinParams;
        _requiredRole = requiredRole;
        _additionRole = additionRole;
    }

    internal bool CanExecute(TelegramCommand command, BotUser user)
    {
        if (_joinParams && _paramCount > 1)
            command = command with
            {
                Parameters = new[]
                {
                    command.Parameters.Join(" ")
                }
            };
        
        return RoleMatches(user)
               && command.Command == _commandName
               && command.Parameters.Length == _paramCount;
    }

    private bool RoleMatches(BotUser user)
    {
        if (_requiredRole == string.Empty && _additionRole == string.Empty)
            return true;

        var result = user.Roles.Contains(_requiredRole);
        if (_additionRole != string.Empty)
        {
            result &= user.Roles.Contains(_additionRole);
        }
        
        return result || user.Roles.Contains("admin");
    }
}