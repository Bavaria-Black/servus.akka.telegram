using LanguageExt;

namespace Servus.Akka.Telegram.Users;

public interface IBotUserRepository
{
    Option<BotUser> GetBotUser(long id);

    Option<bool> HasRole(long id, string role)
    {
        var user = GetBotUser(id);
        return user.BiBind<bool>(u => HasRole(u, role), () => false);
    }

    bool HasRole(BotUser user, string role)
        => user.Roles.Contains(role);

    void AddRole(BotUser user, string role) => AddRole(user.Id, role);
    void AddRole(long id, string role);

    BotUser AddUser(long id, string firstName, string lastName, string username = "", bool isEnabled = true,
        params string[] roles);

    void Save(BotUser user);
}