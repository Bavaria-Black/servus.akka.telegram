namespace Servus.Akka.Telegram.Messages;

public record SendTextMessage(long ChatId, string Message);