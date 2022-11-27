using Servus.Akka.Telegram.Services;
using Servus.Akka.Telegram.Users;

namespace Servus.Akka.Telegram.Messages;

public record CreateNewInvitation(string ActorName, string Role, DateTime ValidUntil, string FirstName, string LastName, string[] UserRoles);
public record CreateNewInvitationResponse(string InvitationLink);
public record InvitationActivated(Invitation Invite, BotUser User);