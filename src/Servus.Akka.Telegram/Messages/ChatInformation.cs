namespace Servus.Akka.Telegram.Messages;

public record ChatInformation(long ChatId, long UserId, string FirstName, string LastName, string Username);