using Akka.Actor;
using Akka.Configuration;
using Akka.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Servus.Akka.Telegram.Hosting;
using Servus.Akka.Telegram.Hosting.Configuration;
using Servus.Akka.Telegram.Messages;
using Servus.Akka.Telegram.Services;
using Servus.Akka.Telegram.Users;
using Telegram.Bot.Requests.Abstractions;

namespace Servus.Akka.Telegram;

public class TelegramIngress : ReceiveActor
{
    private readonly IServiceScope _scope;

    public TelegramIngress(IServiceProvider sp)
    {
        _scope = sp.CreateScope();
        var userRepository = _scope.ServiceProvider.GetRequiredService<IBotUserRepository>();
        var registrationConfig = _scope.ServiceProvider.GetConfiguration<UserRegistrationConfiguration>();
        var botConfig = _scope.ServiceProvider.GetConfiguration<BotConfiguration>();
        var registry = _scope.ServiceProvider.GetRequiredService<IActorRegistry>();
        
        var userRegion = registry.Get<UserShardRegion>();

        Receive<TelegramCommand>(msg =>
        {
            var user = userRepository.GetBotUser(msg.UserId);
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
                var u = userRepository.AddUser(msg.UserId, info.FirstName, info.LastName, info.Username, registrationConfig.EnabledOnStart,
                    registrationConfig.DefaultRoles);

                if (u.IsEnabled)
                {
                    userRegion.Forward(msg);
                }

                if (registrationConfig.NotifyAdminOnUserRegistration)
                {
                    ReplyText(botConfig.AdminUserId, $"Hey how are you? I want to inform you about a new user called {u.GetNameString()}");
                }
            });
        });
    }

    protected void ReplyText(BotUser user, string text)
        => ReplyText(user.Id, text);

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