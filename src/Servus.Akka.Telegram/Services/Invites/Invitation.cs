namespace Servus.Akka.Telegram.Services;

public record Invitation(string Code, DateTime ValidUntil, string Role, string ActorName, string FirstName, string LastName, string[] UserRoles);