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
        _logger.LogDebug("Command [{User}]", User.GetNameString());
    }
}