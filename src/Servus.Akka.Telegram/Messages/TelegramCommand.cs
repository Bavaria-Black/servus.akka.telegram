namespace Servus.Akka.Telegram.Messages;

public record TelegramCommand(long UserId, long MessageId, string Message, string Command, IList<string> Parameters, ChatInformation ChatInformation) : TelegramMessageBase(UserId, MessageId, Message, ChatInformation);
