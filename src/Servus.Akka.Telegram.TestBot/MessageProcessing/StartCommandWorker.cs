using Akka.Actor;
using Microsoft.Extensions.Logging;

namespace Servus.Akka.Telegram.TestBot.MessageProcessing;

public class StartCommandWorker : ReceiveActor
{
    public StartCommandWorker(ILogger<StartCommandWorker> logger)
    {
        ReceiveAny(msg =>
        {
            logger.LogDebug("Hello from the start worker!");
        });
    }
}