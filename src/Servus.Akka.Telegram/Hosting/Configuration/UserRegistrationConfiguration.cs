namespace Servus.Akka.Telegram.Hosting.Configuration;

public class UserRegistrationConfiguration
{
    public const string SectionName = "UserRegistration";

    public bool EnabledOnStart { get; init; } = false;
    public bool NotifyAdminOnUserRegistration { get; init; } = false;
    public string[] DefaultRoles { get; init; } = Array.Empty<string>();
}