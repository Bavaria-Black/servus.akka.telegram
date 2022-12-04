namespace Servus.Akka.Telegram.Services.Invites;

public record Invitation(string Code, DateTime ValidUntil, string Role, string ActorName, string FirstName, string LastName, string[] UserRoles);