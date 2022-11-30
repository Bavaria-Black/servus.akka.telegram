# servus.akka.telegram
Telegram bot using Akka.NET made simple. The goal is to provide an easy to use and flexible Telegram bot framework that lets you have fun programming your bot,
instead of writing a lot of boilerplate code in order to get something done. Have a look at the **Getting started** guide and device yourself if the goal is reached! 

Here is a small list of features it provides:

- Built on top of an akka.net cluster
- Easy bot command handling
- Completely configurable via DI
- Lightweight user management with roles
- Invites for new users

## Getting started

Add the following config sections to your **appsettings.json**

```json
{
  "BotConfiguration" : {
    "BotName": "test",
    "BotLink": "https://t.me/test_bot",
    "BotToken": "1:A-B-_C-D",
    "AdminUserId" : 1
  },
  "UserRegistration": {
    "EnabledOnStart": false,
    "NotifyAdminOnUserRegistration": true,
    "DefaultRoles": ["user"]
  }
}
```

Your **Program.cs** has to look something like the following:  

```csharp
var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services
            .UseTelegramBotService() // register basic services used by the framework
            .AddAkka("TestBot", configurationBuilder =>
            {
                configurationBuilder
                    .ConfigureLoggers(setup =>
                    {
                        setup.LogLevel = LogLevel.DebugLevel;
                        setup.ClearLoggers();
                        setup.AddLoggerFactory();
                        setup.AddLogger<SerilogLogger>();
                    })
                    // initialised the akka.net cluster
                    .AddTelegramCluster(hostname: "localhost", port: 8110, seedNodes: new[] {"akka.tcp://TestBot@localhost:8110"}, additionRoles: "test")
                    .WithActors((system, registry) =>
                    {
                        // register additional actors used by your bot
                        var actor = system.ResolveActor<InviteActivator>("test-invite-activator");
                        registry.Register<InviteActivator>(actor);
                    })
                    // register your command worker with the commands it should handle. 
                    // Optional parameters allow you to prevalidate the command input without you doing anything
                    .WithCommandWorker<HelloCommandWorker>(requiredRole: "user", builder => { builder.AddCommand("/hello"); });
            })
            // You have to provide an implementation for those two interfaces
            .AddScoped<IBotUserRepository, BotUserRepository>()
            .AddScoped<IInviteRepository, InviteRepository>()
    }
```

Finally you have to implement your own `CommandWorker` that receives the bot command and does the things you want it to do :)
It could be as simple as this:

```csharp
public class HelloCommandWorker : CommandWorker
{
    public HelloCommandWorker(BotUser user, ActorRegistry registry, ILogger<StartStopCommandWorker> logger) : base(user, registry, logger)
    {
        RegisterCommand("hello", (_, _) =>
        {
            ReplyText("Hello World!");
        });
    }
}
```

> Hint: You have to implement `IBotUserRepository` and `IInviteRepository` but you can have a look in the TestBot project and copy it from there if you wan't.
