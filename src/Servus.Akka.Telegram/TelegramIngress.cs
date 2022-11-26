using Akka.Actor;
using Akka.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Servus.Akka.Telegram.Messages;
using Servus.Akka.Telegram.Users;

namespace Servus.Akka.Telegram;

public class TelegramIngress : ReceiveActor
{
    private readonly IServiceScope _scope;
    private readonly CommandRegistry _commandRegistry;

    public TelegramIngress(IServiceProvider sp)
    {
        _scope = sp.CreateScope();
        var userRepository = _scope.ServiceProvider.GetRequiredService<IBotUserRepository>();
        _commandRegistry = new CommandRegistry();

        Receive<TelegramCommand>(msg =>
        {
            var user = userRepository.GetBotUser(msg.UserId);
            user.Some(u =>
            {
                if (!_commandRegistry.CheckAndExecute(msg, u))
                {
                    ReplyText(msg.UserId,
                        $"Your command: {msg.Command} {string.Join(' ', msg.Parameters)}, can't be execute right now. Maybe you typed something wrong?");
                }
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

    protected void RegisterCommand(string commandName, int paramCount, bool joinParams, string requiredRole,
        Action<TelegramCommand, BotUser?> action)
        => _commandRegistry.RegisterCommand(commandName, paramCount, joinParams, requiredRole, action);

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