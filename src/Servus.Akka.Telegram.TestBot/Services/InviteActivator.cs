using Akka.Actor;
using Akka.Hosting;
using Microsoft.Extensions.Logging;
using Servus.Akka.Telegram.Messages;
using Servus.Akka.Telegram.TestBot.Repos;

namespace Servus.Akka.Telegram.TestBot.Services;

public class InviteActivator : ReceiveActor
{
    public InviteActivator(ILogger<InviteActivator> logger, TestInviteExtensionRepository repository, ActorRegistry registry)
    {
        var egress = registry.Get<TelegramEgress>();
        
        // 3. User completed the invitation and this actor gets a notification about it. As it was referenced when the invitation was created!
        Receive<InvitationActivated>(msg =>
        {
            logger.LogDebug("Invitation activated!");
            repository.GetExtension(msg.Invite.Code)
                .Some(e =>
                {
                    logger.LogDebug("Extension code was: [{Number}]!", e.Number);
                    egress.Tell(new SendTextMessage(msg.User.Id, $"Hi {msg.User.GetNameString()}. You got assigned to {e.Number}!"));
                })
                .None(() =>
                {
                    logger.LogError("Extension was not found for invite [{InviteCode}]!", msg.Invite.Code);
                    egress.Tell(new SendTextMessage(msg.User.Id, "Your Invitation was not successful. Please ask for a new one!"));
                });
        });
    }
}