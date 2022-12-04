namespace Servus.Akka.Telegram.TestBot;

public class MongoConfiguration
{
    public static readonly string Configuration = "MongoConfiguration";

    public string Host { get; set; } = "";
    public int Port { get; set; } = 27017;
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string Database { get; set; } = "";

    public string GetConnectionString()
        => $"mongodb+srv://{Username}:{Password}@{Host}/{Database}";
}