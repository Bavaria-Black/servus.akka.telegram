namespace Servus.Akka.Telegram.Hosting.Configuration;

public class BotConfiguration
{
    public static readonly string Configuration = "BotConfiguration";

    public string BotToken { get; set; }
    public long AdminUserId { get; set; }
    public string AdminFirstName { get; set; }
    public string AdminLastName { get; set; }
    public string AdminRole { get; set; }
}