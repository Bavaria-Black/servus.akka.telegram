using LanguageExt;

namespace Servus.Akka.Telegram.Services;

public interface IInviteRepository
{
    void InsertInvitation(string code, DateTime validUntil, string role, string actorName, string firstName,
        string lastName, string[] userRoles)
    {
        InsertInvitation(new Invitation(code, validUntil, role, actorName, firstName, lastName, userRoles));
    }
    void InsertInvitation(Invitation invite);

    Option<Invitation> GetInvitation(string code);

    Option<Invitation> TakeInvitation(string code)
    {
        var invite = GetInvitation(code);
        invite.Some(i => DeleteInvitation(code)).None(() => { });
        return invite;
    }
    
    void DeleteInvitation(string code);
}