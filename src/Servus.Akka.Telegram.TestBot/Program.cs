using Akka.Event;
using Akka.Hosting;
using Akka.Logger.Serilog;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using Serilog;
using Serilog.Events;
using Servus.Akka.Telegram.Hosting;
using Servus.Akka.Telegram.Hosting.Configuration;
using Servus.Akka.Telegram.Services.Invites;
using Servus.Akka.Telegram.TestBot;
using Servus.Akka.Telegram.TestBot.Repos;
using Servus.Akka.Telegram.TestBot.Services;
using Servus.Akka.Telegram.TestBot.Worker;
using Servus.Akka.Telegram.Users;
using Telegram.Bot;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

var configBuilder = new ConfigurationBuilder().AddJsonFile("appsettings.json");

if (File.Exists("appsettings.private.json"))
    configBuilder.AddJsonFile("appsettings.private.json", false, true);

var configuration = configBuilder.Build();

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        var conventionPack = new ConventionPack {new IgnoreExtraElementsConvention(true)};
        ConventionRegistry.Register("IgnoreExtraElements", conventionPack, type => true);

        context.Configuration = configuration;

        // Register mongo  configuration
        services.Configure<MongoConfiguration>(configuration.GetSection(MongoConfiguration.Configuration));

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
            .UseTelegramBotService(context)
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
                    .AddTelegramCluster("localhost", 8110, new[] {"akka.tcp://Damask@localhost:8110"}, "test")
                    //.AddHocon("auto-down-unreachable-after")
                    .WithActors((system, registry) =>
                    {
                        var actor = system.ResolveActor<InviteActivator>("test-invite-activator");
                        registry.Register<InviteActivator>(actor);
                    })
                    .WithCommandWorker<InviteCreationWorker>("admin", builder => { builder.AddCommand("/test", 1); })
                    .WithCommandWorker<HelloCommandWorker>("admin", builder => { builder.AddCommand("hello"); })
                    .WithCommandWorker<StartStopCommandWorker>("test",
                        builder => { builder.AddCommand("/begin").AddCommand("end").AddCommand("time"); });
            })
            .AddScoped<IBotUserRepository, BotUserRepository>()
            .AddScoped<IInviteRepository, InviteRepository>()
            .AddScoped<TestInviteExtensionRepository>()
            .AddScoped(s =>
            {
                var options = s.GetConfiguration<MongoConfiguration>();
                return new MongoClient(options.GetConnectionString());
            })
            .AddScoped(s => s.GetRequiredService<MongoClient>().GetDatabase("damask"))
            .AddScoped(s => s.GetRequiredService<IMongoDatabase>().GetCollection<BotUser>("botuser"))
            .AddScoped(s => s.GetRequiredService<IMongoDatabase>().GetCollection<Invitation>("invites"))
            .AddScoped(s =>
                s.GetRequiredService<IMongoDatabase>().GetCollection<TestInviteExtension>("invite-extension"));
    })
    .UseSerilog()
    .Build();

await host.RunAsync();