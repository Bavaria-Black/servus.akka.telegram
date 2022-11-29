using Akka.Actor;
using Akka.Hosting;
using Akka.Persistence;
using Microsoft.Extensions.Logging;
using Servus.Akka.Telegram.Messages;
using Servus.Akka.Telegram.Users;

namespace Servus.Akka.Telegram;

internal class CommandMap
{
    private readonly List<CommandMapEntry> _commands = new();

    private class CommandMapEntry
    {
        private readonly string _command;
        private readonly Action<IList<string>, ChatInformation> _action;

        public CommandMapEntry(string command, Action<IList<string>, ChatInformation> action)
        {
            if (!command.StartsWith("/"))
                command = $"/{command}";
            
            _command = command;
            _action = action;
        }

        public void Execute(CommandMessage msg)
        {
            if (msg.Command == _command)
            {
                _action(msg.Arguments, msg.ChatInformation);
            }
        }
    }

    public void Add(string command, Action<IList<string>, ChatInformation> action)
        => _commands.Add(new CommandMapEntry(command, action));

    public void Execute(CommandMessage msg)
    {
        foreach (var commandMap in _commands)
        {
            commandMap.Execute(msg);
        }
    }
}

public abstract class CommandWorker : ReceiveActor
{
    protected BotUser User { get; }
    protected readonly ILogger _logger;
    private readonly IActorRef _egress;
    private readonly CommandMap _commandMap = new();

    protected CommandWorker(BotUser user, ActorRegistry registry, ILogger logger)
    {
        User = user;

        _logger = logger;
        _egress = registry.Get<TelegramEgress>();

        Receive<CommandMessage>(_commandMap.Execute);
    }

    protected void RegisterCommand(string command, Action<IList<string>, ChatInformation> action) 
        => _commandMap.Add(command, action);

    protected void ReplyText(string text)
    {
        _egress.Tell(new SendTextMessage(User.Id, text));
    }
}

public abstract class PersistentCommandWorker : ReceivePersistentActor
{
    protected BotUser User { get; }
    protected readonly ILogger _logger;
    private readonly IActorRef _egress;
    private readonly CommandMap _commandMap = new();

    protected PersistentCommandWorker(BotUser user, ActorRegistry registry, ILogger logger)
    {
        User = user;

        _logger = logger;
        _egress = registry.Get<TelegramEgress>();

        Command<CommandMessage>(_commandMap.Execute);
    }

    protected void RegisterCommand(string command, Action<IList<string>, ChatInformation> action) 
        => _commandMap.Add(command, action);

    protected void ReplyText(string text)
    {
        _egress.Tell(new SendTextMessage(User.Id, text));
    }
}