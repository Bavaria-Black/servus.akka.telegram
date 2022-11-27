using Akka.Hosting;
using Microsoft.Extensions.Logging;
using Servus.Akka.Telegram.Messages;
using Servus.Akka.Telegram.Users;

namespace Servus.Akka.Telegram.TestBot.CommandWorker;

public class StartCommandWorker : Telegram.CommandWorker
{
    public StartCommandWorker(BotUser user, ActorRegistry registry, ILogger<StartCommandWorker> logger) : base(user, registry, logger)
    {
    }
    
    protected override void ProcessCommand(IList<string> args, ChatInformation chatInfo)
    {
        switch (args.Count)
        {
            case 0:
                _logger.LogDebug("New user [{User}] joined", User.GetNameString());
                break;
            case 1:
                _logger.LogDebug("New user [{User}] joined via invite", User.GetNameString());
                break;
            default:
                _logger.LogWarning("New user [{User}] joined but i don't know how....", User.GetNameString());
                break;
        }
    }
}