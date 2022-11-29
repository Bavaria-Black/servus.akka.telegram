namespace Servus.Akka.Telegram.Messages;

internal record CommandMessage(string Command, ChatInformation ChatInformation, IList<string> Arguments);