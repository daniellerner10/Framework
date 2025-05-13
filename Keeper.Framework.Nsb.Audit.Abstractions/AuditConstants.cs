namespace Keeper.Framework.Nsb.Audit.Abstractions;

internal static class AuditConstants
{
    public const string AuditMessageFieldName = "AuditMessage";
    public const bool AuditMessageFieldValue = true;

    public const string AuditMessageText = "Message";
    public const string BodyHeader = "Body";
    public const string ReceivingEndpointHeader = "KEEPER.ReceivingEndpoint";
    public const string SendDelayMillisecondsHeader = "KEEPER.SendDelayMilliseconds";
    public const string MessageDirectionHeader = "KEEPER.MessageDirection";
    public const string MessageSuccessfulHeader = "KEEPER.MessageSuccessful";
    public const string MovedToErrorQueueHeader = "KEEPER.MovedToErrorQueue";
    public const string IncomingMessage = "IncomingMessage";
    public const string OutgoingMessage = "OutgoingMessage";

    public const string KeeperAuditIncomingStepName = "KeeperAuditIncomingStepName";
}
