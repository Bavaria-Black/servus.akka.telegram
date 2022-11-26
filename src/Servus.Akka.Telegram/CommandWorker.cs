using Akka.Actor;
using Microsoft.Extensions.Logging;
using Servus.Akka.Telegram.Messages;
using Servus.Akka.Telegram.Users;

namespace Servus.Akka.Telegram;

public abstract class CommandWorker : ReceiveActor
{
    protected BotUser User { get; }
    protected readonly ILogger _logger;

    protected CommandWorker(BotUser user, ILogger logger)
    {
        User = user;
        _logger = logger;
        Receive<CommandMessage>(msg =>
        {
            ProcessCommand(msg.Arguments, msg.ChatInformation);
        });
    }

    protected abstract void ProcessCommand(IList<string> args, ChatInformation chatInfo);
}