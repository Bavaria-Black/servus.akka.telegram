using Akka.Actor;
using Servus.Akka.Telegram.Registry;

namespace Servus.Akka.Telegram.Hosting;

public class WorkerRegistryExtension : ExtensionIdProvider<WorkerRegistry>
{
    public override WorkerRegistry CreateExtension(ExtendedActorSystem system)
    {
        return new WorkerRegistry();
    }
}