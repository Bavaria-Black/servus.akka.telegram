using Akka.Actor;
using Akka.Hosting;
using Akka.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Servus.Akka.Telegram.Messages;
using Servus.Akka.Telegram.Registry;
using Servus.Akka.Telegram.Users;
using Telegram.Bot.Types;

namespace Servus.Akka.Telegram.Services;

public class UserShardRegion : ReceiveActor
{
    private readonly long _userId;
    private readonly ILogger<UserShardRegion> _logger;
    private readonly IServiceScope _scope;
    private readonly IBotUserRepository _userRepository;
    private BotUser _user = new();
    private readonly WorkerRegistry _commandRegistry;
    private readonly IActorRef _egress;

    public UserShardRegion(long userId, IServiceProvider sp, ILogger<UserShardRegion> logger)
    {
        logger.LogDebug("Staring up new shard entity for user {UserId}", userId);

        _userId = userId;
        _logger = logger;
        _scope = sp.CreateScope();
        _userRepository = _scope.ServiceProvider.GetRequiredService<IBotUserRepository>();
        _commandRegistry = WorkerRegistry.For(Context.System);
        
        var registry = _scope.ServiceProvider.GetRequiredService<IActorRegistry>();
        _egress = registry.Get<TelegramEgress>();
    }
    protected override void PreStart()
    {
        _userRepository.GetBotUser(_userId).Some(u =>
        { 
            _user = u;
            if (_user.IsEnabled)
            {
                Become(Ready);
            }
            else if(!_user.IsBanned)
            {
                Become(Disabled);
            }
        }).None(() => Become(NewUser));

        _userRepository.GetBotUser(_userId);
    }

    private void Ready()
    {
        _logger.LogDebug("BECOME ready for user [{UserId}] [{UserName}]", _userId, _user.GetNameString());

        Receive<TelegramCommand>(msg =>
        {
            _logger.LogDebug("Known user received message of type {TypeName}", msg.GetType().Name);
            var executed = _commandRegistry.CheckAndExecute(msg, _user, (propFac, workerId) =>
            {
                var worker = Context.Child(workerId);
                if (worker.IsNobody())
                {
                    var prop = propFac(_user);
                    _logger.LogDebug("Creating new worker [{WorkerType}] for command [{TelegramCommand}] with [{ArgumentCount}] arguments", prop.TypeName, msg.Command, msg.Parameters.Count);
                    worker = Context.ActorOf(prop, workerId);
                }
                
                worker.Tell(new CommandMessage(msg.ChatInformation, msg.Parameters));
            });

            if (!executed)
            {
                _egress.Tell(new SendTextMessage(msg.UserId, "Unrecognized command. Say what?"));
            }
        });
        
        ReceiveAny(msg =>
        {
            _logger.LogDebug("Known user received unhandled message of type {TypeName}", msg.GetType().Name);    
        });   
    }

    private void Disabled()
    {
        _logger.LogDebug("BECOME disabled for user [{UserId}] [{UserName}]", _userId, _user.GetNameString());
        
        ReceiveAny(msg =>
        {
            _logger.LogWarning("User [{UserId}] [{UserName}] is currently disabled. All messaged being dropped!", _userId, _user.GetNameString());            
        });   
    }

    private void NewUser()
    {
        ReceiveAny(msg =>
        {
            _logger.LogDebug("Unknown user received message of type {TypeName}", msg.GetType().Name);
            if (msg is TelegramMessageBase messageBase)
            {
                var chat = messageBase.ChatInformation;
                _user = _userRepository.AddUser(_userId, chat.FirstName, chat.LastName, chat.Username, false);
                Become(Ready);
            }
        });
    }

    protected override void PostStop()
    {
        _scope.Dispose();
    }
}