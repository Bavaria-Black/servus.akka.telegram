using Akka.Actor;
using Akka.Cluster.Routing;
using Akka.Hosting;
using Akka.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Servus.Akka.Telegram.Hosting;
using Servus.Akka.Telegram.Hosting.Configuration;
using Servus.Akka.Telegram.Messages;
using Servus.Akka.Telegram.Services;
using Servus.Akka.Telegram.Services.Invites;
using Servus.Akka.Telegram.Users;

namespace Servus.Akka.Telegram;

public class TelegramIngress : ReceiveActor
{
    private readonly ILogger<TelegramIngress> _logger;
    private readonly IServiceScope _scope;
    private readonly IBotUserRepository _userRepository;
    private readonly IInviteRepository _inviteRepository;
    private readonly UserRegistrationConfiguration _registrationConfig;

    public TelegramIngress(IServiceProvider sp, ILogger<TelegramIngress> logger)
    {
        _logger = logger;
        _scope = sp.CreateScope();
        _userRepository = _scope.ServiceProvider.GetRequiredService<IBotUserRepository>();
        _inviteRepository = _scope.ServiceProvider.GetRequiredService<IInviteRepository>();
        _registrationConfig = _scope.ServiceProvider.GetConfiguration<UserRegistrationConfiguration>();
        var botConfig = _scope.ServiceProvider.GetConfiguration<BotConfiguration>();
        var registry = _scope.ServiceProvider.GetRequiredService<IActorRegistry>();

        var userRegion = registry.Get<UserShardRegion>();

        Receive<TelegramText>(msg => { ReplyText(msg.UserId, $"Sorry I only understand commands for now..."); });

        Receive<TelegramCommand>(msg =>
        {
            if (msg.Command == "/start" && msg.Parameters.Length == 1)
            {
                // user invite!
                logger.LogDebug("Processing new invite....");
                ProcessInvite(msg);
                return;
            }

            var user = _userRepository.GetBotUser(msg.UserId);
            user.Some(u =>
            {
                if (u.IsBanned) return;
                if (!u.IsEnabled)
                {
                    ReplyText(msg.UserId, $"Sorry you are currently disabled. Please come back later.");
                    return;
                }

                userRegion.Forward(msg);
            }).None(() =>
            {
                if (msg.Command != "/start")
                {
                    ReplyText(msg.UserId, $"Sorry I don't know you. You can try to use /start");
                    return;
                }

                ReplyText(msg.UserId, $"Hello stranger! Welcome to {botConfig.BotName}!");

                var info = msg.ChatInformation;
                var u = _userRepository.AddUser(msg.UserId, info.FirstName, info.LastName, info.Username,
                    _registrationConfig.EnabledOnStart,
                    _registrationConfig.DefaultRoles);

                if (u.IsEnabled)
                {
                    userRegion.Forward(msg);
                }

                if (_registrationConfig.NotifyAdminOnUserRegistration)
                {
                    ReplyText(botConfig.AdminUserId,
                        $"Hey how are you? I want to inform you about a new user called {u.GetNameString()}");
                }
            });
        });
    }

    private void ProcessInvite(TelegramCommand msg)
    {
        var userOption = _userRepository.GetBotUser(msg.UserId);
        var inviteOption = _inviteRepository.TakeInvitation(msg.Parameters.First());
        inviteOption.Some(invite =>
        {
            if (invite.ValidUntil < DateTime.UtcNow)
            {
                ReplyText(msg.UserId, "Sorry your invite expired");
                return;
            }

            userOption
                .Some(user =>
                {
                    _logger.LogDebug("Adding roles [{Roles}] to user [{User}]", string.Join(", ", invite.UserRoles),
                        user.GetNameString());
                    _userRepository.AddRoles(user, invite.UserRoles);
                })
                .None(() =>
                {
                    var newRoles = _registrationConfig.DefaultRoles.Union(invite.UserRoles).ToArray();
                    var user = _userRepository.AddUser(msg.UserId, invite.FirstName, invite.LastName,
                        msg.ChatInformation.Username,
                        _registrationConfig.EnabledOnStart,
                        _registrationConfig.DefaultRoles.Union(invite.UserRoles).ToArray());
                    _logger.LogDebug("Adding new user [{User}] with roles [{Roles}]", user.GetNameString(),
                        string.Join(", ", newRoles));
                });

            var inviteActorName = $"invite-{invite.ActorName}Router";
            var inviteActor = Context.Child(inviteActorName);
            if (inviteActor.IsNobody())
            {
                _logger.LogDebug("Creating router for [{InviteActorName}]", inviteActorName);
                inviteActor = Context.ActorOf(
                    Props.Empty.WithRouter(new ClusterRouterGroup(
                        new ConsistentHashingGroup(new[] {"/user/" + invite.ActorName}, m => m),
                        new ClusterRouterGroupSettings(10000, new[] {"/user/" + invite.ActorName},
                            true, useRole: invite.Role))), inviteActorName);
            }

            _userRepository.GetBotUser(msg.UserId)
                .Some(user =>
                {
                    Self.Tell(msg with
                    {
                        Parameters = Array.Empty<string>()
                    });
                    
                    _logger.LogDebug("Sending message to incite actor [{InviteActorName}]", inviteActorName);
                    inviteActor.Tell(new InvitationActivated(invite, user));
                })
                .None(() =>
                {
                    _logger.LogError("Error while activating the invitation... User was not created or updated");
                });
        }).None(() => { ReplyText(msg.UserId, "Sorry your invite probably expired"); });
    }

    protected void ReplyText(long chatId, string text)
    {
        var gateway = ActorRegistry.For(Context.System).Get<TelegramEgress>();
        gateway.Tell(new SendTextMessage(chatId, text));
    }

    protected override void PostStop()
    {
        _scope.Dispose();
    }
}