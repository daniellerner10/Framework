using NServiceBus.Extensibility;

namespace Keeper.Framework.Extensions.Nsb;

public static class NsbSendOnlyExtensions
{
    /// <summary>
    /// Send a message and associate the NServiceBus.ConversationId header with the correlationId.
    /// If you pass an idempotency key, this method associates it with the message id.
    /// </summary>
    /// <typeparam name="T">The type of message to build.</typeparam>
    /// <param name="messageSession">The message session.</param>
    /// <param name="messageConstructor">The message constructor.</param>
    /// <param name="correlationId">The correlation id.</param>
    /// <param name="idempotencyKey">The idempotency key.</param>
    /// <param name="options">The send options.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task to wait on.</returns>
    public static Task SendWithCorrelation<T>(this IMessageSession messageSession, Action<T> messageConstructor, string correlationId, string? idempotencyKey = null, SendOptions? options = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(messageSession);
        ArgumentNullException.ThrowIfNull(messageConstructor);
        ArgumentNullException.ThrowIfNull(correlationId);

        return SendOrPublishWithCorrelation(
            messageConstructor,
            correlationId,
            options,
            messageSession.Send, 
            idempotencyKey,
            cancellationToken);
    }

    /// <summary>
    /// Send a message and associate the NServiceBus.ConversationId header with the correlationId.
    /// If you pass an idempotency key, this method associates it with the message id.
    /// </summary>
    /// <param name="messageSession">The message session.</param>
    /// <param name="message">The message.</param>
    /// <param name="correlationId">The correlation id.</param>
    /// <param name="idempotencyKey">The idempotency key.</param>
    /// <param name="options">The send options.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task to wait on.</returns>
    public static Task SendWithCorrelation(this IMessageSession messageSession, object message, string correlationId, string? idempotencyKey = null, SendOptions? options = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(messageSession);
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(correlationId);

        return SendOrPublishWithCorrelation(
            message, 
            correlationId, 
            options, 
            messageSession.Send, 
            idempotencyKey,
            cancellationToken);
    }

    /// <summary>
    /// Send a message and associate the NServiceBus.ConversationId header with the correlationId.
    /// If you pass an idempotency key, this method associates it with the message id.
    /// </summary>
    /// <typeparam name="T">The type of message to build.</typeparam>
    /// <param name="messageSession">The message session.</param>
    /// <param name="destination">The destination endpoint.</param>
    /// <param name="messageConstructor">The message constructor.</param>
    /// <param name="correlationId">The correlation id.</param>
    /// <param name="idempotencyKey">The idempotency key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task to wait on.</returns>
    public static Task SendWithCorrelation<T>(this IMessageSession messageSession, string destination, Action<T> messageConstructor, string correlationId, string? idempotencyKey = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(messageSession);
        ArgumentNullException.ThrowIfNull(messageConstructor);
        ArgumentNullException.ThrowIfNull(correlationId);

        return SendWithCorrelation(
            destination,
            messageConstructor,
            correlationId,
            messageSession.Send,
            idempotencyKey,
            cancellationToken);
    }

    /// <summary>
    /// Send a message to the local endpoint and associate the NServiceBus.ConversationId header with the correlationId.
    /// If you pass an idempotency key, this method associates it with the message id.
    /// </summary>
    /// <typeparam name="T">The type of message to build.</typeparam>
    /// <param name="messageSession">The message session.</param>
    /// <param name="messageConstructor">The message constructor.</param>
    /// <param name="correlationId">The correlation id.</param>
    /// <param name="idempotencyKey">The idempotency key.</param>
    /// <param name="options">The send options.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task to wait on.</returns>
    public static Task SendLocalWithCorrelation<T>(this IMessageSession messageSession, Action<T> messageConstructor, string correlationId, string? idempotencyKey = null, SendOptions? options = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(messageSession);
        ArgumentNullException.ThrowIfNull(messageConstructor);
        ArgumentNullException.ThrowIfNull(correlationId);

        return SendLocalWithCorrelation(
            messageConstructor,
            correlationId,
            options,
            messageSession.Send,
            idempotencyKey,
            cancellationToken);
    }

    /// <summary>
    /// Send a message to the local endpoint and associate the NServiceBus.ConversationId header with the correlationId.
    /// If you pass an idempotency key, this method associates it with the message id.
    /// </summary>
    /// <param name="messageSession">The message session.</param>
    /// <param name="message">The message.</param>
    /// <param name="correlationId">The correlation id.</param>
    /// <param name="idempotencyKey">The idempotency key.</param>
    /// <param name="options">The send options.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task to wait on.</returns>
    public static Task SendLocalWithCorrelation(this IMessageSession messageSession, object message, string correlationId, string? idempotencyKey = null, SendOptions? options = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(messageSession);
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(correlationId);

        return SendLocalWithCorrelation(
            message,
            correlationId,
            options,
            messageSession.Send,
            idempotencyKey,
            cancellationToken);
    }

    /// <summary>
    /// Publish a message and associate the NServiceBus.ConversationId header with the correlationId.
    /// If you pass an idempotency key, this method associates it with the message id.
    /// </summary>
    /// <typeparam name="T">The type of message to build.</typeparam>
    /// <param name="messageSession">The message session.</param>
    /// <param name="messageConstructor">The message constructor.</param>
    /// <param name="correlationId">The correlation id.</param>
    /// <param name="idempotencyKey">The idempotency key.</param>
    /// <param name="options">The publish options</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task to wait on.</returns>
    public static Task PublishWithCorrelation<T>(this IMessageSession messageSession, Action<T> messageConstructor, string correlationId, string? idempotencyKey = null, PublishOptions? options = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(messageSession);
        ArgumentNullException.ThrowIfNull(messageConstructor);
        ArgumentNullException.ThrowIfNull(correlationId);

        return SendOrPublishWithCorrelation(
            messageConstructor,
            correlationId,
            options,
            (x, y, ct) => messageSession.Publish(x, y, ct),
            idempotencyKey,
            cancellationToken);
    }

    /// <summary>
    /// Publish a message and associate the NServiceBus.ConversationId header with the correlationId.
    /// If you pass an idempotency key, this method associates it with the message id.
    /// </summary>
    /// <param name="messageSession">The message session.</param>
    /// <param name="message">The message.</param>
    /// <param name="correlationId">The correlation id.</param>
    /// <param name="idempotencyKey">The idempotency key.</param>
    /// <param name="options">The publish options</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task to wait on.</returns>
    public static Task PublishWithCorrelation(this IMessageSession messageSession, object message, string correlationId, string? idempotencyKey = null, PublishOptions? options = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(messageSession);
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(correlationId);

        return SendOrPublishWithCorrelation(
            message,
            correlationId,
            options,
            messageSession.Publish, idempotencyKey, cancellationToken);
    }

    private static Task SendLocalWithCorrelation<TArg>(
        TArg message,
        string correlationId,
        SendOptions? options,
        Func<TArg, SendOptions, CancellationToken, Task> sendFunc,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new SendOptions();
        options.RouteToThisEndpoint();

        return SendOrPublishWithCorrelation(message, correlationId, options, sendFunc, idempotencyKey, cancellationToken);
    }

    private static Task SendWithCorrelation<TArg>(
        string destination,
        TArg message,
        string correlationId,
        Func<TArg, SendOptions, CancellationToken, Task> sendFunc,
        string? idempotencyKey = null, 
        CancellationToken cancellationToken = default)
    {
        var options = new SendOptions();
        options.SetDestination(destination);

        return SendOrPublishWithCorrelation(message, correlationId, options, sendFunc, idempotencyKey, cancellationToken);
    }

    private static Task SendOrPublishWithCorrelation<TArg, TOptions, CancellationToken>(
        TArg message,
        string correlationId,
        TOptions? options,
        Func<TArg, TOptions, CancellationToken, Task> sendOrPublishFunc,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default!)
        where TOptions : ExtendableOptions, new()
    {
        if (options is null)
            options = new TOptions();

        options.SetHeader(Headers.ConversationId, correlationId);
        if (!string.IsNullOrWhiteSpace(idempotencyKey))
            options.SetMessageId(idempotencyKey);

        return sendOrPublishFunc(message, options, cancellationToken);
    }
}
