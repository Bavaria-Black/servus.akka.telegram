using Akka.Actor;
using Microsoft.Extensions.DependencyInjection;
using Servus.Akka.Telegram.Messages;
using Telegram.Bot;

namespace Servus.Akka.Telegram;

public class TelegramEgress : ReceiveActor
{
    private readonly IServiceScope _scope;

    public TelegramEgress(IServiceProvider sp)
    {
        _scope = sp.CreateScope();
        var botClient = _scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

        ReceiveAsync<SendTextMessage>(async msg =>
        {
            var message = await botClient.SendTextMessageAsync(msg.ChatId, msg.Message);
        });
    }

    protected override void PostStop()
    {
        _scope.Dispose();
    }
}