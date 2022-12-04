namespace Servus.Akka.Telegram.Messages;

internal abstract record CommandMessageBase(string Command, ChatInformation ChatInformation);
internal record CommandMessage(string Command, ChatInformation ChatInformation, IList<string> Arguments)
    : CommandMessageBase(Command, ChatInformation);

internal record IncompleteCommandMessage(string Command, ChatInformation ChatInformation, IList<string> Arguments)
    : CommandMessageBase(Command, ChatInformation);