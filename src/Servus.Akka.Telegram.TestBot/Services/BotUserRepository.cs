using LanguageExt;
using MongoDB.Driver;
using Servus.Akka.Telegram.Users;

namespace Servus.Akka.Telegram.TestBot.Services;

public class BotUserRepository : IBotUserRepository
{
    private readonly IMongoCollection<BotUser> _collection;

    public BotUserRepository(IMongoCollection<BotUser> collection)
    {
        _collection = collection;
    }
    
    public Option<BotUser> GetBotUser(long id)
    {
        return _collection.Find(f => f.Id == id).ToList().FirstOrDefault();
    }

    public void AddRole(long id, string role)
    {
        _ = GetBotUser(id).Some(user =>
        {
            user.Roles.Add(role);
            _collection.UpdateOne(f => f.Id == user.Id,
                Builders<BotUser>.Update.Set(f => f.Roles, user.Roles));
        });
    }

    public BotUser AddUser(long id, string firstName, string lastName, string username = "", bool isEnabled = true,
        params string[] roles)
    {
        var user = new BotUser()
        {
            Id = id,
            Roles = new List<string>(roles),
            FirstName = firstName,
            LastName = lastName,
            NickName = username,
            IsEnabled = isEnabled
        };
        
        _collection.InsertOne(user);

        return user;
    }

    public void Save(BotUser user)
    {
        _collection.ReplaceOne(f => f.Id == user.Id, user);
    }
}