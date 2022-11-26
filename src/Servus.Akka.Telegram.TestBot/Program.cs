using Akka.Actor;
using Akka.Cluster.Hosting;
using Akka.DependencyInjection;
using Akka.Event;
using Akka.Hosting;
using Akka.Logger.Serilog;
using Akka.Remote.Hosting;
using Akka.Util;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using Serilog;
using Serilog.Events;
using Servus.Akka.Telegram;
using Servus.Akka.Telegram.Hosting;
using Servus.Akka.Telegram.Hosting.Configuration;
using Servus.Akka.Telegram.Messages;
using Servus.Akka.Telegram.Services;
using Servus.Akka.Telegram.TestBot;
using Servus.Akka.Telegram.TestBot.MessageProcessing;
using Servus.Akka.Telegram.TestBot.Services;
using Servus.Akka.Telegram.Users;
using Telegram.Bot;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(app =>
    {
        if (File.Exists("appsettings.private.json"))
            app.AddJsonFile("appsettings.private.json", false, true);
    })
    .ConfigureServices((context, services) =>
    {
        var conventionPack = new ConventionPack {new IgnoreExtraElementsConvention(true)};
        ConventionRegistry.Register("IgnoreExtraElements", conventionPack, type => true);

        // Register Bot configuration
        services.Configure<BotConfiguration>(
            context.Configuration.GetSection(BotConfiguration.SectionName));
        // Register Bot configuration
        services.Configure<UserRegistrationConfiguration>(
            context.Configuration.GetSection(UserRegistrationConfiguration.SectionName));
        // Register mongo  configuration
        services.Configure<MongoConfiguration>(
            context.Configuration.GetSection(MongoConfiguration.Configuration));

        // Register named HttpClient to benefits from IHttpClientFactory
        // and consume it with ITelegramBotClient typed client.
        // More read:
        //  https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-5.0#typed-clients
        //  https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests
        services.AddHttpClient("telegram_bot_client")
            .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
            {
                TelegramBotClientOptions botClientOptions = new(sp.GetConfiguration<BotConfiguration>().BotToken);
                return new TelegramBotClient(botClientOptions, httpClient);
            });
        services
            .UseTelegramBotService()
            .AddAkka("Damask", configurationBuilder =>
            {
                configurationBuilder
                    .ConfigureLoggers(setup =>
                    {
                        setup.LogLevel = LogLevel.DebugLevel;
                        setup.ClearLoggers();
                        setup.AddLoggerFactory();
                        setup.AddLogger<SerilogLogger>();
                    })
                    .WithRemoting("localhost", 8110)
                    .WithClustering(new ClusterOptions()
                    {
                        Roles = new[] {"myRole", "user-shard"},
                        SeedNodes = new[] {Address.Parse("akka.tcp://Damask@localhost:8110")}
                    })
                    .AddHocon("auto-down-unreachable-after")
                    .WithActors((system, registry) =>
                    {
                        registry.TryRegisterDiActor<TelegramIngress>(system, "telegram-ingress");
                        registry.TryRegisterDiActor<TelegramEgress>(system, "telegram-egress");
                    }).WithShardRegion<UserShardRegion>(
                        typeName: "user-region",
                        compositePropsFactory: (system, registry) =>
                        {
                            return (e) => DependencyResolver.For(system).Props<UserShardRegion>(long.Parse(e));
                        },
                        new UserMessageExtractor(5),
                        new ShardOptions()
                        {
                            Role = "user-shard"
                        }
                    ).WithCommandWorker<StartCommandWorker>("/start", 1, false, string.Empty);
            })
            .AddScoped<IBotUserRepository, BotUserRepository>()
            .AddScoped(s =>
            {
                var options = s.GetRequiredService<IOptions<MongoConfiguration>>();
                return new MongoClient(options.Value.GetConnectionString());
            })
            .AddScoped(s => s.GetRequiredService<MongoClient>().GetDatabase("damask"))
            .AddScoped(s => s.GetRequiredService<IMongoDatabase>().GetCollection<BotUser>("botuser"));
    })
    .UseSerilog()
    .Build();

await host.RunAsync();

namespace Servus.Akka.Telegram.TestBot
{
    public class MongoConfiguration
    {
        public static readonly string Configuration = "MongoConfiguration";

        public string Host { get; set; } = "cluster0.xvksw1g.mongodb.net";
        public int Port { get; set; } = 27017;
        public string Username { get; set; } = "damask";
        public string Password { get; set; } = "niAVhuI5feCap0gt";
        public string Database { get; set; } = "damask";

        public string GetConnectionString()
            => $"mongodb+srv://{Username}:{Password}@{Host}/{Database}";
    }
}