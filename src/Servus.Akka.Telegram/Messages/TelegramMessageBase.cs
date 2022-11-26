namespace Servus.Akka.Telegram.Messages;

public record TelegramMessageBase(long UserId, long MessageId, string Message, ChatInformation ChatInformation);