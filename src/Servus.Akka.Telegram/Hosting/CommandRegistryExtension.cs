using Akka.Actor;
using Akka.Hosting;

namespace Servus.Akka.Telegram.Hosting;

public class CommandRegistryExtension : ExtensionIdProvider<CommandRegistry>
{
    public override CommandRegistry CreateExtension(ExtendedActorSystem system)
    {
        return new CommandRegistry();
    }
}