using Akka.Actor;
using Akka.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Servus.Akka.Telegram.Messages;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Servus.Akka.Telegram.Services;

public class MessageUpdateService : IHostedService
{
    private readonly ILogger<MessageUpdateService> _logger;
    private readonly IServiceScope _scope;
    private readonly ITelegramBotClient _bot;
    private readonly CancellationTokenSource _cts;
    private readonly ActorRegistry _registry;

    public MessageUpdateService(IServiceProvider sp, ILogger<MessageUpdateService> logger)
    {
        _logger = logger;
        _cts = new CancellationTokenSource();
        _scope = sp.CreateScope();
        _bot = _scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();
        _registry = _scope.ServiceProvider.GetRequiredService<ActorRegistry>();
    }


    public Task StartAsync(CancellationToken cancellationToken)
    {
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
        };

        _bot.StartReceiving(
            updateHandler: OnMessageUpdate,
            pollingErrorHandler: OnPollingError,
            receiverOptions: receiverOptions,
            cancellationToken: _cts.Token
        );

        return Task.CompletedTask;
    }

    private Task OnPollingError(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(ErrorMessage);
        return Task.CompletedTask;
    }

    private Task OnMessageUpdate(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        // Only process Message updates: https://core.telegram.org/bots/api#message
        if (update.Message is not { } message)
            return Task.CompletedTask;

        _logger.LogDebug("Received a [{UpdateType}|{MessageType}] from {ChatId}: {Text}", update.Type,
            update.Message.Type,
            update.Message.Chat.Id, update.Message.Text);

        // Only process text messages
        if (message.Text is not { } messageText)
            return Task.CompletedTask;

        // Only process text messages
        if (message.From is not { } from)
            return Task.CompletedTask;

        var chatInfo =
            new ChatInformation(update.Message.Chat.Id, from.Id, from.FirstName, from.LastName ?? string.Empty,
                from.Username ?? string.Empty);

        var chatId = message.Chat.Id;
        if (message.Entities?.Length > 0)
        {
            var actor = _registry.Get<TelegramIngress>();
            for (var i = 0; i < message.Entities.Length; i++)
            {
                var entity = message.Entities[i];
                _logger.LogDebug("Offset [{Offset}], Length [{Length}]", entity.Offset, entity.Length);
                var command = message.Text.Substring(entity.Offset, entity.Length);
                var parameter = Array.Empty<string>();

                if (i == message.Entities.Length - 1)
                {
                    parameter = message.Text.Substring(entity.Offset + entity.Length).Split(' ');
                }
                else
                {
                    var start = entity.Offset + entity.Length;
                    parameter = message.Text
                        .Substring(start, message.Entities[i + 1].Offset - start)
                        .Split(' ');
                }

                parameter = parameter.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                _logger.LogDebug("Received a [{EntityType}] from {ChatId}: [{Command}] [{CommandParameter}]",
                    entity.Type, update.Message.Chat.Id, command, parameter);
                actor.Tell(new TelegramCommand(chatId, message.MessageId, messageText, command, parameter, chatInfo));
            }
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _scope?.Dispose();
        _cts?.Cancel();
        _cts?.Dispose();

        return Task.CompletedTask;
    }
}