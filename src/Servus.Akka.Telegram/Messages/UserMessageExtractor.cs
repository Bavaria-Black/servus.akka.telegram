using Akka.Cluster.Sharding;

namespace Servus.Akka.Telegram.Messages;

public class UserMessageExtractor : HashCodeMessageExtractor
{
    public UserMessageExtractor(int maxNumberOfShards) : base(maxNumberOfShards)
    {
    }

    public override string EntityId(object message)
    {
        return message switch
        {
            TelegramMessageBase messageBase => messageBase.UserId.ToString(),
            _ => string.Empty
        };
    }
}