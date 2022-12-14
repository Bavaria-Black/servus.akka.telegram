using Akka.Actor;
using Akka.Hosting;
using Microsoft.Extensions.Logging;
using Servus.Akka.Telegram.Messages;
using Servus.Akka.Telegram.Services.Invites;
using Servus.Akka.Telegram.TestBot.Repos;
using Servus.Akka.Telegram.Users;

namespace Servus.Akka.Telegram.TestBot.Worker;

public class InviteCreationWorker : CommandWorker
{
    private readonly TestInviteExtensionRepository _repository;
    private readonly IActorRef _invitationController;
    private int _inviteParameter = 0;

    public InviteCreationWorker(BotUser user, ActorRegistry registry, ILogger<InviteCreationWorker> logger, TestInviteExtensionRepository repository) : base(user, registry, logger)
    {
        _repository = repository;
        _invitationController = registry.Get<InvitationController>();

        // 2. The invitation controller created the invitation code.
        // Here you can extend the basic invitation with your own stuff. e.g. store additional information in a DB
        Receive<CreateNewInvitationResponse>(msg =>
        {
            // Send the invite link to the user that has requested it            
            ReplyText($"Here is your freshly generated invitation link: {msg.InvitationLink}");

            // extend the invite with additional information needed for activation
            _repository.Insert(msg.Code, _inviteParameter);
        });
        
        RegisterCommand("/test", ProcessCommand);
        
        RegisterIncompleteCommand("/test", (list, _) =>
        {
            ReplyText($"It is one parameter required to execute your command! You supplied: {list.Count}]");
        });
    }

    private void ProcessCommand(IList<string> args, ChatInformation chatInfo)
    {
        if (int.TryParse(args.First(), out _inviteParameter))
        {
            // 1. tell the invitation controller to create a new invitation. The actor name and role is for the actor
            //    that will be notified when the invitation got accepted.
            _invitationController.Tell(new CreateNewInvitation("test-invite-activator", "test", DateTime.UtcNow.AddDays(1),
                "Test", "User", new[] {"test"}));            
        }
        
    }
}