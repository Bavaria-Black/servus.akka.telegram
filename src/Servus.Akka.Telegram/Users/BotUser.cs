namespace Servus.Akka.Telegram.Users;

public class BotUser
{
    public long Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string NickName { get; init; } = string.Empty;
    
    public List<string> Roles { get; init; } = new ();
    
    public bool IsEnabled { get; set; }

    public string GetNameString() => string.Join(" ", FirstName, LastName);
}