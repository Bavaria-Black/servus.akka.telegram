using Akka.Actor;
using Microsoft.Extensions.Logging;
using MongoDB.Driver.Core.WireProtocol.Messages;
using Servus.Akka.Telegram.Messages;
using Servus.Akka.Telegram.Users;

namespace Servus.Akka.Telegram.TestBot.MessageProcessing;

public class StartCommandWorker : CommandWorker
{
    public StartCommandWorker(BotUser user, ILogger<StartCommandWorker> logger) : base(user, logger)
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