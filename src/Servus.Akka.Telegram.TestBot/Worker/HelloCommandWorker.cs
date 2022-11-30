using Akka.Hosting;
using Microsoft.Extensions.Logging;
using Servus.Akka.Telegram.Users;

namespace Servus.Akka.Telegram.TestBot.CommandWorker;

public class HelloCommandWorker : Telegram.CommandWorker
{
    public HelloCommandWorker(BotUser user, ActorRegistry registry, ILogger<StartStopCommandWorker> logger) : base(user, registry, logger)
    {
        RegisterCommand("hello", (_, _) =>
        {
            ReplyText("Hello World!");
        });
    }
}