namespace Servus.Akka.Telegram.Messages;

public record TelegramCommand(long UserId, long MessageId, string Message, string Command, string[] Parameters, ChatInformation ChatInformation) : TelegramMessageBase(UserId, MessageId, Message, ChatInformation);
public record TelegramText(long UserId, long MessageId, string Message, ChatInformation ChatInformation) : TelegramMessageBase(UserId, MessageId, Message, ChatInformation);
