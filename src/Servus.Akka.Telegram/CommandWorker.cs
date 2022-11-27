using Akka.Actor;
using Akka.Hosting;
using Microsoft.Extensions.Logging;
using Servus.Akka.Telegram.Messages;
using Servus.Akka.Telegram.Users;

namespace Servus.Akka.Telegram;

public abstract class CommandWorker : ReceiveActor
{
    protected BotUser User { get; }
    protected readonly ILogger _logger;
    private readonly IActorRef _egress;

    protected CommandWorker(BotUser user, ActorRegistry registry, ILogger logger)
    {
        User = user;
        
        _logger = logger;
        _egress = registry.Get<TelegramEgress>();
        
        Receive<CommandMessage>(msg =>
        {
            ProcessCommand(msg.Arguments, msg.ChatInformation);
        });
    }

    protected void ReplyText(string text)
    {
        _egress.Tell(new SendTextMessage(User.Id, text));
    }

    protected abstract void ProcessCommand(IList<string> args, ChatInformation chatInfo);
}