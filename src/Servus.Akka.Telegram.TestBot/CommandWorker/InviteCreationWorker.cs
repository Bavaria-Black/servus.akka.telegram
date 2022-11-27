using Akka.Actor;
using Akka.Hosting;
using Microsoft.Extensions.Logging;
using Servus.Akka.Telegram.Messages;
using Servus.Akka.Telegram.Services;
using Servus.Akka.Telegram.Users;

namespace Servus.Akka.Telegram.TestBot.CommandWorker;

public class InviteCreationWorker : Telegram.CommandWorker
{
    private readonly IActorRef _invitationController;

    public InviteCreationWorker(BotUser user, ActorRegistry registry, ILogger<InviteCreationWorker> logger) : base(user, registry, logger)
    {
        _invitationController = registry.Get<InvitationController>();

        Receive<CreateNewInvitationResponse>(msg =>
        {
            ReplyText($"Here is your freshly generated invitation link: {msg.InvitationLink}");
        });
    }

    protected override void ProcessCommand(IList<string> args, ChatInformation chatInfo)
    {
        _invitationController.Tell(new CreateNewInvitation("test-invite-activator", "test", DateTime.UtcNow.AddDays(1),
            "Test", "User", new[] {"test"}));
    }
}