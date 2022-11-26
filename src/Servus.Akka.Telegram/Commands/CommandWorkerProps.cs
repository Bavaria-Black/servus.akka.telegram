using Akka.Actor;

namespace Servus.Akka.Telegram.Commands;

public class CommandWorkerProps
{
    private readonly string _command;
    private readonly Props _props;

    public CommandWorkerProps(string command, Props props)
    {
        _command = command;
        _props = props;
    }
}