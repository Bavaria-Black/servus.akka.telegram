namespace Servus.Akka.Telegram.Hosting.Configuration;

public class BotConfiguration
{
    public static readonly string SectionName = "BotConfiguration";

    public string BotToken { get; set; } = string.Empty;

    public string BotName { get; set; } = string.Empty;
    public string BotLink { get; set; } = string.Empty;
    public long AdminUserId { get; set; }
    public string AdminFirstName { get; set; } = string.Empty;
    public string AdminLastName { get; set; } = string.Empty;
    public string AdminRole { get; set; } = string.Empty;
}