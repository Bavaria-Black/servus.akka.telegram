using Akka.Actor;
using Microsoft.Extensions.Logging;
using Servus.Akka.Telegram.Messages;

namespace Servus.Akka.Telegram.TestBot.Services;

public class InviteActivator : ReceiveActor
{
    public InviteActivator(ILogger<InviteActivator> logger)
    {
        Receive<InvitationActivated>(msg =>
        {
            logger.LogDebug("Invitation activated!");
        });
    }
}