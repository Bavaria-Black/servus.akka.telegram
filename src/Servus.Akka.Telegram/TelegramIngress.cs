using Akka.Actor;
using Akka.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Servus.Akka.Telegram.Messages;
using Servus.Akka.Telegram.Services;
using Servus.Akka.Telegram.Users;

namespace Servus.Akka.Telegram;

public class TelegramIngress : ReceiveActor
{
    private readonly IServiceScope _scope;

    public TelegramIngress(IServiceProvider sp)
    {
        _scope = sp.CreateScope();
        var userRepository = _scope.ServiceProvider.GetRequiredService<IBotUserRepository>();
        var registry = _scope.ServiceProvider.GetRequiredService<IActorRegistry>();
        
        var userRegion = registry.Get<UserShardRegion>();

        Receive<TelegramCommand>(msg =>
        {
            var user = userRepository.GetBotUser(msg.UserId);
            user.Some(u =>
            {
                userRegion.Forward(msg);
            }).None(() =>
            {

                if (msg.Command != "/start")
                {
                    ReplyText(msg.UserId, $"Sorry I don't know you");
                    return;
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