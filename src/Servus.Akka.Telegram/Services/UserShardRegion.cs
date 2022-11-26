using Akka.Actor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Servus.Akka.Telegram.Messages;
using Servus.Akka.Telegram.Users;

namespace Servus.Akka.Telegram.Services;

public class UserShardRegion : ReceiveActor
{
    private readonly long _userId;
    private readonly ILogger<UserShardRegion> _logger;
    private readonly IServiceScope _scope;
    private readonly IBotUserRepository _userRepository;
    private BotUser _user = new();

    public UserShardRegion(long userId, IServiceProvider sp, ILogger<UserShardRegion> logger)
    {
        _userId = userId;
        _logger = logger;
        _scope = sp.CreateScope();
        _userRepository = _scope.ServiceProvider.GetRequiredService<IBotUserRepository>();
        _logger.LogDebug("Staring up new shard entity for user {UserId}", userId);      
    }
    protected override void PreStart()
    {
        _userRepository.GetBotUser(_userId).Some(u =>
        { 
            _user = u;
            Become(Ready);    
        }).None(() => Become(NewUser));

        _userRepository.GetBotUser(_userId);
    }

    private void Ready()
    {
        _logger.LogDebug("BECOME ready for user [{UserId}] [{UserName}]", _userId, _user.GetNameString());
        
        ReceiveAny(msg =>
        {
            _logger.LogDebug("Known user received message of type {TypeName}", msg.GetType().Name);            
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