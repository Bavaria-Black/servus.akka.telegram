using Akka.Actor;
using Microsoft.Extensions.DependencyInjection;
using Servus.Akka.Telegram.Hosting;
using Servus.Akka.Telegram.Hosting.Configuration;
using Servus.Akka.Telegram.Messages;

namespace Servus.Akka.Telegram.Services.Invites;

public class InvitationController : ReceiveActor
{
    private readonly IServiceScope _scope;

    public InvitationController(IServiceProvider sp)
    {
        _scope = sp.CreateScope();
        var config = _scope.ServiceProvider.GetConfiguration<BotConfiguration>();
        var inviteRepo = _scope.ServiceProvider.GetRequiredService<IInviteRepository>();

        Receive<CreateNewInvitation>(msg =>
        {
            var code = InviteCodeGenerator.CreateInviteCode();

            inviteRepo.InsertInvitation(code, msg.ValidUntil, msg.Role, msg.ActorName, msg.FirstName, msg.LastName,
                msg.UserRoles);
            
            Sender.Tell(new CreateNewInvitationResponse(string.Join("/", config.BotLink, $"?start={code}"), code));
        });
    }

    protected override void PostStop()
    {
        _scope?.Dispose();
    }
}