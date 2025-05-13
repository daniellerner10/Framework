namespace Keeper.Framework.Logging;

/// <summary>
/// Keeper logging properties names.
/// </summary>
public static class KeeperLoggingProperty
{
    /// <summary>
    /// The message id.
    /// </summary>
    public const string MessageId = "MessageId";

    /// <summary>
    /// The Keeper correlation id.
    /// </summary>
    public const string KeeperCorrelationId = "KeeperCorrelationId";

    /// <summary>
    /// The idempotency key
    /// </summary>
    public const string IdempotencyKey = "IdempotencyKey";

    /// <summary>
    /// The timestamp of when the message was sent.
    /// </summary>
    public const string MessageTimeSent = "MessageTimeSent";

    /// <summary>
    /// The timestamp of when the message started processing.
    /// </summary>
    public const string MessageProcessingStarted = "MessageProcessingStarted";

    /// <summary>
    /// Message processing time in milliseconds.
    /// </summary>
    public const string MessageProcessingTimeMilliseconds = "MessageProcessingTimeMilliseconds";

    /// <summary>
    /// The milliseconds between the message being sent and the time it was received.
    /// </summary>
    public const string MessageSentToProcessingMilliseconds = "MessageSentToProcessingMilliseconds";

    /// <summary>
    /// The type of the message.
    /// </summary>
    public const string MessageType = "MessageType";

    /// <summary>
    /// Whether the message is a saga timeout message.
    /// </summary>
    public const string MessageIsSagaTimeout = "MessageIsSagaTimeout";

    /// <summary>
    /// The delay in seconds of the timeout message.
    /// </summary>
    public const string MessageTimeoutDelayInSeconds = "MessageTimeoutDelayInSeconds";

    /// <summary>
    /// Whether the message was successful or not.
    /// </summary>
    public const string MessageIsSuccessful = "MessageIsSuccessful";
}