namespace Servus.Akka.Telegram.Messages;

public record CommandMessage(ChatInformation ChatInformation, IList<string> Arguments);