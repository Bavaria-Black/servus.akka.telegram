using Akka.Dispatch.SysMsg;
using Akka.Hosting;
using Microsoft.Extensions.Logging;
using Servus.Akka.Telegram.Messages;
using Servus.Akka.Telegram.Users;

namespace Servus.Akka.Telegram.TestBot.CommandWorker;

public class StartStopCommandWorker : PersistentCommandWorker
{
    public record Start(DateTime Time);
    public record End(DateTime Time);
    
    public override string PersistenceId { get; }

    private DateTime _startTime = DateTime.MinValue;

    public StartStopCommandWorker(BotUser user, ActorRegistry registry, ILogger<StartStopCommandWorker> logger) : base(user, registry, logger)
    {
        PersistenceId = $"StartStop-{user.Id}";

        Recover<Start>(start => _startTime = start.Time);
        Recover<End>(end => _startTime = DateTime.MinValue);
        
        RegisterCommand("begin", (_, _) =>
        {
            if (_startTime != DateTime.MinValue)
            {
                ReplyText("Your time is already running...");
                return;
            }
            
            Persist(new Start(DateTime.UtcNow), start =>
            {
                _logger.LogDebug("Started at {CurrentTime}", start.Time);
                _startTime = start.Time;
                ReplyText("Your time running...");
            });
        });
        
        RegisterCommand("end", (_, _) =>
        {
            if (_startTime == DateTime.MinValue)
            {
                ReplyText("You haven't started yet...");
                return;
            }
            
            Persist(new End(DateTime.UtcNow), end =>
            {
                var runTime = end.Time-_startTime;
                _startTime = DateTime.MinValue;
                _logger.LogDebug("End at {CurrentTime}, with a runtime of {Runtime}", end.Time, runTime);
                ReplyText($"You ended your time after: {runTime}");
            });
        });
        
        RegisterCommand("time", (_, _) =>
        {
            if (_startTime == DateTime.MinValue)
            {
                ReplyText("No time available because you haven't started yet...");
                return;
            }                
            
            ReplyText($"Your current time is: {DateTime.UtcNow-_startTime}");
        });
    }
}