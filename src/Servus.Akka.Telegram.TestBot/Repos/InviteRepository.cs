using LanguageExt;
using MongoDB.Driver;
using Servus.Akka.Telegram.Services.Invites;

namespace Servus.Akka.Telegram.TestBot.Repos;

public class InviteRepository : IInviteRepository
{
    private readonly IMongoCollection<Invitation> _inviteCollection;

    public InviteRepository(IMongoCollection<Invitation> inviteCollection)
    {
        _inviteCollection = inviteCollection;
    }
    
    public void InsertInvitation(Invitation invite)
    {
        _inviteCollection.InsertOne(invite);
    }

    public Option<Invitation> GetInvitation(string code)
    {
        return _inviteCollection.FindSync(f => f.Code == code).ToList().FirstOrDefault();
    }

    public void DeleteInvitation(string code)
    {
        _inviteCollection.DeleteOne(f => f.Code == code);
    }
}