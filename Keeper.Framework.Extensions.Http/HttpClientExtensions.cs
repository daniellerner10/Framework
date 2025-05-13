using Keeper.Framework.Application.State;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;

namespace Keeper.Framework.Extensions.Http;
/// <summary>
/// The http client extensions.
/// </summary>
public static class HttpClientExtensions
{
    private const string CONTENT_TYPE_HEADER = "content-type";
    private const string DEFAULT_CONTENT_TYPE = "application/json";

    /// <summary>
    /// Adds the System.Net.Http.IHttpClientFactory and related services to the Microsoft.Extensions.DependencyInjection.IServiceCollection.
    /// Also enriches every HttpClient with the current keeper correlation id if it is available.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddKeeperHttpClient(this IServiceCollection services)
    {
        AddKeeperHttpClient(() => services.AddHttpClient(Options.DefaultName));
        return services;
    }

    /// <summary>
    /// Adds the <see cref="IHttpClientFactory"/> and related services to the <see cref="IServiceCollection"/> and configures
    /// a named <see cref="HttpClient"/>.
    /// Also enriches every HttpClient with the current keeper correlation id if it is available.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="name">The logical name of the <see cref="HttpClient"/> to configure.</param>
    /// <returns>An <see cref="IHttpClientBuilder"/> that can be used to configure the client.</returns>
    /// <remarks>
    /// <para>
    /// <see cref="HttpClient"/> instances that apply the provided configuration can be retrieved using
    /// <see cref="IHttpClientFactory.CreateClient(string)"/> and providing the matching name.
    /// </para>
    /// <para>
    /// Use <see cref="Options.DefaultName"/> as the name to configure the default client.
    /// </para>
    /// </remarks>
    public static IHttpClientBuilder AddKeeperHttpClient(this IServiceCollection services, string name) =>
        AddKeeperHttpClient(() => services.AddHttpClient(name));

    /// <summary>
    /// Adds the <see cref="IHttpClientFactory"/> and related services to the <see cref="IServiceCollection"/> and configures
    /// a named <see cref="HttpClient"/>.
    /// Also enriches every HttpClient with the current Keeper correlation id if it is available.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="name">The logical name of the <see cref="HttpClient"/> to configure.</param>
    /// <param name="configureClient">A delegate that is used to configure an <see cref="HttpClient"/>.</param>
    /// <returns>An <see cref="IHttpClientBuilder"/> that can be used to configure the client.</returns>
    /// <remarks>
    /// <para>
    /// <see cref="HttpClient"/> instances that apply the provided configuration can be retrieved using
    /// <see cref="IHttpClientFactory.CreateClient(string)"/> and providing the matching name.
    /// </para>
    /// <para>
    /// Use <see cref="Options.DefaultName"/> as the name to configure the default client.
    /// </para>
    /// </remarks>
    public static IHttpClientBuilder AddKeeperHttpClient(this IServiceCollection services, string name, Action<HttpClient> configureClient) =>
        AddKeeperHttpClient(() => services.AddHttpClient(name, configureClient));

    /// <summary>
    /// Adds the <see cref="IHttpClientFactory"/> and related services to the <see cref="IServiceCollection"/> and configures
    /// a named <see cref="HttpClient"/>.
    /// Also enriches every HttpClient with the current Keeper correlation id if it is available.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="name">The logical name of the <see cref="HttpClient"/> to configure.</param>
    /// <param name="configureClient">A delegate that is used to configure an <see cref="HttpClient"/>.</param>
    /// <returns>An <see cref="IHttpClientBuilder"/> that can be used to configure the client.</returns>
    /// <remarks>
    /// <para>
    /// <see cref="HttpClient"/> instances that apply the provided configuration can be retrieved using
    /// <see cref="IHttpClientFactory.CreateClient(string)"/> and providing the matching name.
    /// </para>
    /// <para>
    /// Use <see cref="Options.DefaultName"/> as the name to configure the default client.
    /// </para>
    /// </remarks>
    public static IHttpClientBuilder AddKeeperHttpClient(this IServiceCollection services, string name, Action<IServiceProvider, HttpClient> configureClient) =>
        AddKeeperHttpClient(() => services.AddHttpClient(name, configureClient));

    /// <summary>
    /// Adds the <see cref="IHttpClientFactory"/> and related services to the <see cref="IServiceCollection"/> and configures
    /// a binding between the <typeparamref name="TClient"/> type and a named <see cref="HttpClient"/>. The client name
    /// will be set to the type name of <typeparamref name="TClient"/>.
    /// Also enriches every HttpClient with the current Keeper correlation id if it is available.
    /// </summary>
    /// <typeparam name="TClient">
    /// The type of the typed client. The type specified will be registered in the service collection as
    /// a transient service. See <see cref="ITypedHttpClientFactory{TClient}" /> for more details about authoring typed clients.
    /// </typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <returns>An <see cref="IHttpClientBuilder"/> that can be used to configure the client.</returns>
    /// <remarks>
    /// <para>
    /// <see cref="HttpClient"/> instances that apply the provided configuration can be retrieved using
    /// <see cref="IHttpClientFactory.CreateClient(string)"/> and providing the matching name.
    /// </para>
    /// <para>
    /// <typeparamref name="TClient"/> instances constructed with the appropriate <see cref="HttpClient" />
    /// can be retrieved from <see cref="IServiceProvider.GetService(Type)" /> (and related methods) by providing
    /// <typeparamref name="TClient"/> as the service type.
    /// </para>
    /// </remarks>
    /// 
    public static IHttpClientBuilder AddKeeperHttpClient<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TClient>(
        this IServiceCollection services)
        where TClient : class =>
        AddKeeperHttpClient(() => services.AddHttpClient<TClient>());

    /// <summary>
    /// Adds the <see cref="IHttpClientFactory"/> and related services to the <see cref="IServiceCollection"/> and configures
    /// a binding between the <typeparamref name="TClient" /> type and a named <see cref="HttpClient"/>. The client name will
    /// be set to the type name of <typeparamref name="TClient"/>.
    /// Also enriches every HttpClient with the current Keeper correlation id if it is available.
    /// </summary>
    /// <typeparam name="TClient">
    /// The type of the typed client. The type specified will be registered in the service collection as
    /// a transient service. See <see cref="ITypedHttpClientFactory{TClient}" /> for more details about authoring typed clients.
    /// </typeparam>
    /// <typeparam name="TImplementation">
    /// The implementation type of the typed client. The type specified will be instantiated by the
    /// <see cref="ITypedHttpClientFactory{TImplementation}"/>
    /// </typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <returns>An <see cref="IHttpClientBuilder"/> that can be used to configure the client.</returns>
    /// <remarks>
    /// <para>
    /// <see cref="HttpClient"/> instances that apply the provided configuration can be retrieved using
    /// <see cref="IHttpClientFactory.CreateClient(string)"/> and providing the matching name.
    /// </para>
    /// <para>
    /// <typeparamref name="TClient"/> instances constructed with the appropriate <see cref="HttpClient" />
    /// can be retrieved from <see cref="IServiceProvider.GetService(Type)" /> (and related methods) by providing
    /// <typeparamref name="TClient"/> as the service type.
    /// </para>
    /// </remarks>
    public static IHttpClientBuilder AddKeeperHttpClient<TClient, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(
        this IServiceCollection services)
        where TClient : class
        where TImplementation : class, TClient =>
        AddKeeperHttpClient(() => services.AddHttpClient<TClient, TImplementation>());

    /// <summary>
    /// Adds the <see cref="IHttpClientFactory"/> and related services to the <see cref="IServiceCollection"/> and configures
    /// a binding between the <typeparamref name="TClient"/> type and a named <see cref="HttpClient"/>.
    /// Also enriches every HttpClient with the current Keeper correlation id if it is available.
    /// </summary>
    /// <typeparam name="TClient">
    /// The type of the typed client. The type specified will be registered in the service collection as
    /// a transient service. See <see cref="ITypedHttpClientFactory{TClient}" /> for more details about authoring typed clients.
    /// </typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="name">The logical name of the <see cref="HttpClient"/> to configure.</param>
    /// <returns>An <see cref="IHttpClientBuilder"/> that can be used to configure the client.</returns>
    /// <remarks>
    /// <para>
    /// <see cref="HttpClient"/> instances that apply the provided configuration can be retrieved using
    /// <see cref="IHttpClientFactory.CreateClient(string)"/> and providing the matching name.
    /// </para>
    /// <para>
    /// <typeparamref name="TClient"/> instances constructed with the appropriate <see cref="HttpClient" />
    /// can be retrieved from <see cref="IServiceProvider.GetService(Type)" /> (and related methods) by providing
    /// <typeparamref name="TClient"/> as the service type.
    /// </para>
    /// <para>
    /// Use <see cref="Options.DefaultName"/> as the name to configure the default client.
    /// </para>
    /// </remarks>
    public static IHttpClientBuilder AddKeeperHttpClient<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TClient>(
        this IServiceCollection services, string name)
        where TClient : class =>
        AddKeeperHttpClient(() => services.AddHttpClient<TClient>(name));

    /// <summary>
    /// Adds the <see cref="IHttpClientFactory"/> and related services to the <see cref="IServiceCollection"/> and configures
    /// a binding between the <typeparamref name="TClient" /> type and a named <see cref="HttpClient"/>. The client name will
    /// be set to the type name of <typeparamref name="TClient"/>.
    /// Also enriches every HttpClient with the current Keeper correlation id if it is available.
    /// </summary>
    /// <typeparam name="TClient">
    /// The type of the typed client. The type specified will be registered in the service collection as
    /// a transient service. See <see cref="ITypedHttpClientFactory{TClient}" /> for more details about authoring typed clients.
    /// </typeparam>
    /// <typeparam name="TImplementation">
    /// The implementation type of the typed client. The type specified will be instantiated by the
    /// <see cref="ITypedHttpClientFactory{TImplementation}"/>
    /// </typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="name">The logical name of the <see cref="HttpClient"/> to configure.</param>
    /// <returns>An <see cref="IHttpClientBuilder"/> that can be used to configure the client.</returns>
    /// <remarks>
    /// <para>
    /// <see cref="HttpClient"/> instances that apply the provided configuration can be retrieved using
    /// <see cref="IHttpClientFactory.CreateClient(string)"/> and providing the matching name.
    /// </para>
    /// <para>
    /// <typeparamref name="TClient"/> instances constructed with the appropriate <see cref="HttpClient" />
    /// can be retrieved from <see cref="IServiceProvider.GetService(Type)" /> (and related methods) by providing
    /// <typeparamref name="TClient"/> as the service type.
    /// </para>
    /// <para>
    /// Use <see cref="Options.DefaultName"/> as the name to configure the default client.
    /// </para>
    /// </remarks>
    public static IHttpClientBuilder AddKeeperHttpClient<TClient, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(
        this IServiceCollection services, string name)
        where TClient : class
        where TImplementation : class, TClient =>
        AddKeeperHttpClient(() => services.AddHttpClient<TClient, TImplementation>(name));

    /// <summary>
    /// Adds the <see cref="IHttpClientFactory"/> and related services to the <see cref="IServiceCollection"/> and configures
    /// a binding between the <typeparamref name="TClient" /> type and a named <see cref="HttpClient"/>. The client name will
    /// be set to the type name of <typeparamref name="TClient"/>.
    /// Also enriches every HttpClient with the current Keeper correlation id if it is available.
    /// </summary>
    /// <typeparam name="TClient">
    /// The type of the typed client. The type specified will be registered in the service collection as
    /// a transient service. See <see cref="ITypedHttpClientFactory{TClient}" /> for more details about authoring typed clients.
    /// </typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="configureClient">A delegate that is used to configure an <see cref="HttpClient"/>.</param>
    /// <returns>An <see cref="IHttpClientBuilder"/> that can be used to configure the client.</returns>
    /// <remarks>
    /// <para>
    /// <see cref="HttpClient"/> instances that apply the provided configuration can be retrieved using
    /// <see cref="IHttpClientFactory.CreateClient(string)"/> and providing the matching name.
    /// </para>
    /// <para>
    /// <typeparamref name="TClient"/> instances constructed with the appropriate <see cref="HttpClient" />
    /// can be retrieved from <see cref="IServiceProvider.GetService(Type)" /> (and related methods) by providing
    /// <typeparamref name="TClient"/> as the service type.
    /// </para>
    /// </remarks>
    public static IHttpClientBuilder AddKeeperHttpClient<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TClient>(
        this IServiceCollection services, Action<HttpClient> configureClient)
        where TClient : class =>
        AddKeeperHttpClient(() => services.AddHttpClient<TClient>(configureClient));

    /// <summary>
    /// Adds the <see cref="IHttpClientFactory"/> and related services to the <see cref="IServiceCollection"/> and configures
    /// a binding between the <typeparamref name="TClient" /> type and a named <see cref="HttpClient"/>. The client name will
    /// be set to the type name of <typeparamref name="TClient"/>.
    /// Also enriches every HttpClient with the current Keeper correlation id if it is available.
    /// </summary>
    /// <typeparam name="TClient">
    /// The type of the typed client. The type specified will be registered in the service collection as
    /// a transient service. See <see cref="ITypedHttpClientFactory{TClient}" /> for more details about authoring typed clients.
    /// </typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="configureClient">A delegate that is used to configure an <see cref="HttpClient"/>.</param>
    /// <returns>An <see cref="IHttpClientBuilder"/> that can be used to configure the client.</returns>
    /// <remarks>
    /// <para>
    /// <see cref="HttpClient"/> instances that apply the provided configuration can be retrieved using
    /// <see cref="IHttpClientFactory.CreateClient(string)"/> and providing the matching name.
    /// </para>
    /// <para>
    /// <typeparamref name="TClient"/> instances constructed with the appropriate <see cref="HttpClient" />
    /// can be retrieved from <see cref="IServiceProvider.GetService(Type)" /> (and related methods) by providing
    /// <typeparamref name="TClient"/> as the service type.
    /// </para>
    /// </remarks>
    public static IHttpClientBuilder AddKeeperHttpClient<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TClient>(
        this IServiceCollection services, Action<IServiceProvider, HttpClient> configureClient)
        where TClient : class =>
        AddKeeperHttpClient(() => services.AddHttpClient<TClient>(configureClient));

    /// <summary>
    /// Adds the <see cref="IHttpClientFactory"/> and related services to the <see cref="IServiceCollection"/> and configures
    /// a binding between the <typeparamref name="TClient" /> type and a named <see cref="HttpClient"/>. The client name will
    /// be set to the type name of <typeparamref name="TClient"/>.
    /// Also enriches every HttpClient with the current Keeper correlation id if it is available.
    /// </summary>
    /// <typeparam name="TClient">
    /// The type of the typed client. The type specified will be registered in the service collection as
    /// a transient service. See <see cref="ITypedHttpClientFactory{TClient}" /> for more details about authoring typed clients.
    /// </typeparam>
    /// <typeparam name="TImplementation">
    /// The implementation type of the typed client. The type specified will be instantiated by the
    /// <see cref="ITypedHttpClientFactory{TImplementation}"/>
    /// </typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="configureClient">A delegate that is used to configure an <see cref="HttpClient"/>.</param>
    /// <returns>An <see cref="IHttpClientBuilder"/> that can be used to configure the client.</returns>
    /// <remarks>
    /// <para>
    /// <see cref="HttpClient"/> instances that apply the provided configuration can be retrieved using
    /// <see cref="IHttpClientFactory.CreateClient(string)"/> and providing the matching name.
    /// </para>
    /// <para>
    /// <typeparamref name="TClient"/> instances constructed with the appropriate <see cref="HttpClient" />
    /// can be retrieved from <see cref="IServiceProvider.GetService(Type)" /> (and related methods) by providing
    /// <typeparamref name="TClient"/> as the service type.
    /// </para>
    /// </remarks>
    public static IHttpClientBuilder AddKeeperHttpClient<TClient, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(
        this IServiceCollection services, Action<HttpClient> configureClient)
        where TClient : class
        where TImplementation : class, TClient =>
        AddKeeperHttpClient(() => services.AddHttpClient<TClient, TImplementation>(configureClient));

    /// <summary>
    /// Adds the <see cref="IHttpClientFactory"/> and related services to the <see cref="IServiceCollection"/> and configures
    /// a binding between the <typeparamref name="TClient" /> type and a named <see cref="HttpClient"/>. The client name will
    /// be set to the type name of <typeparamref name="TClient"/>.
    /// Also enriches every HttpClient with the current Keeper correlation id if it is available.
    /// </summary>
    /// <typeparam name="TClient">
    /// The type of the typed client. The type specified will be registered in the service collection as
    /// a transient service. See <see cref="ITypedHttpClientFactory{TClient}" /> for more details about authoring typed clients.
    /// </typeparam>
    /// <typeparam name="TImplementation">
    /// The implementation type of the typed client. The type specified will be instantiated by the
    /// <see cref="ITypedHttpClientFactory{TImplementation}"/>
    /// </typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="configureClient">A delegate that is used to configure an <see cref="HttpClient"/>.</param>
    /// <returns>An <see cref="IHttpClientBuilder"/> that can be used to configure the client.</returns>
    /// <remarks>
    /// <para>
    /// <see cref="HttpClient"/> instances that apply the provided configuration can be retrieved using
    /// <see cref="IHttpClientFactory.CreateClient(string)"/> and providing the matching name.
    /// </para>
    /// <para>
    /// <typeparamref name="TClient"/> instances constructed with the appropriate <see cref="HttpClient" />
    /// can be retrieved from <see cref="IServiceProvider.GetService(Type)" /> (and related methods) by providing
    /// <typeparamref name="TClient"/> as the service type.
    /// </para>
    /// </remarks>
    public static IHttpClientBuilder AddKeeperHttpClient<TClient, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(
        this IServiceCollection services, Action<IServiceProvider, HttpClient> configureClient)
        where TClient : class
        where TImplementation : class, TClient =>
        AddKeeperHttpClient(() => services.AddHttpClient<TClient, TImplementation>(configureClient));

    /// <summary>
    /// Adds the <see cref="IHttpClientFactory"/> and related services to the <see cref="IServiceCollection"/> and configures
    /// a binding between the <typeparamref name="TClient" /> type and a named <see cref="HttpClient"/>.
    /// Also enriches every HttpClient with the current Keeper correlation id if it is available.
    /// </summary>
    /// <typeparam name="TClient">
    /// The type of the typed client. The type specified will be registered in the service collection as
    /// a transient service. See <see cref="ITypedHttpClientFactory{TClient}" /> for more details about authoring typed clients.
    /// </typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="name">The logical name of the <see cref="HttpClient"/> to configure.</param>
    /// <param name="configureClient">A delegate that is used to configure an <see cref="HttpClient"/>.</param>
    /// <returns>An <see cref="IHttpClientBuilder"/> that can be used to configure the client.</returns>
    /// <remarks>
    /// <para>
    /// <see cref="HttpClient"/> instances that apply the provided configuration can be retrieved using
    /// <see cref="IHttpClientFactory.CreateClient(string)"/> and providing the matching name.
    /// </para>
    /// <para>
    /// <typeparamref name="TClient"/> instances constructed with the appropriate <see cref="HttpClient" />
    /// can be retrieved from <see cref="IServiceProvider.GetService(Type)" /> (and related methods) by providing
    /// <typeparamref name="TClient"/> as the service type.
    /// </para>
    /// <para>
    /// Use <see cref="Options.DefaultName"/> as the name to configure the default client.
    /// </para>
    /// </remarks>
    public static IHttpClientBuilder AddKeeperHttpClient<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TClient>(
        this IServiceCollection services, string name, Action<HttpClient> configureClient)
        where TClient : class =>
        AddKeeperHttpClient(() => services.AddHttpClient<TClient>(name, configureClient));

    /// <summary>
    /// Adds the <see cref="IHttpClientFactory"/> and related services to the <see cref="IServiceCollection"/> and configures
    /// a binding between the <typeparamref name="TClient" /> type and a named <see cref="HttpClient"/>.
    /// Also enriches every HttpClient with the current Keeper correlation id if it is available.
    /// </summary>
    /// <typeparam name="TClient">
    /// The type of the typed client. The type specified will be registered in the service collection as
    /// a transient service. See <see cref="ITypedHttpClientFactory{TClient}" /> for more details about authoring typed clients.
    /// </typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="name">The logical name of the <see cref="HttpClient"/> to configure.</param>
    /// <param name="configureClient">A delegate that is used to configure an <see cref="HttpClient"/>.</param>
    /// <returns>An <see cref="IHttpClientBuilder"/> that can be used to configure the client.</returns>
    /// <remarks>
    /// <para>
    /// <see cref="HttpClient"/> instances that apply the provided configuration can be retrieved using
    /// <see cref="IHttpClientFactory.CreateClient(string)"/> and providing the matching name.
    /// </para>
    /// <para>
    /// <typeparamref name="TClient"/> instances constructed with the appropriate <see cref="HttpClient" />
    /// can be retrieved from <see cref="IServiceProvider.GetService(Type)" /> (and related methods) by providing
    /// <typeparamref name="TClient"/> as the service type.
    /// </para>
    /// <para>
    /// Use <see cref="Options.DefaultName"/> as the name to configure the default client.
    /// </para>
    /// </remarks>
    public static IHttpClientBuilder AddKeeperHttpClient<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TClient>(
        this IServiceCollection services, string name, Action<IServiceProvider, HttpClient> configureClient)
        where TClient : class =>
        AddKeeperHttpClient(() => services.AddHttpClient<TClient>(name, configureClient));

    /// <summary>
    /// Adds the <see cref="IHttpClientFactory"/> and related services to the <see cref="IServiceCollection"/> and configures
    /// a binding between the <typeparamref name="TClient" /> type and a named <see cref="HttpClient"/>.
    /// Also enriches every HttpClient with the current Keeper correlation id if it is available.
    /// </summary>
    /// <typeparam name="TClient">
    /// The type of the typed client. The type specified will be registered in the service collection as
    /// a transient service. See <see cref="ITypedHttpClientFactory{TClient}" /> for more details about authoring typed clients.
    /// </typeparam>
    /// <typeparam name="TImplementation">
    /// The implementation type of the typed client. The type specified will be instantiated by the
    /// <see cref="ITypedHttpClientFactory{TImplementation}"/>
    /// </typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="name">The logical name of the <see cref="HttpClient"/> to configure.</param>
    /// <param name="configureClient">A delegate that is used to configure an <see cref="HttpClient"/>.</param>
    /// <returns>An <see cref="IHttpClientBuilder"/> that can be used to configure the client.</returns>
    /// <remarks>
    /// <para>
    /// <see cref="HttpClient"/> instances that apply the provided configuration can be retrieved using
    /// <see cref="IHttpClientFactory.CreateClient(string)"/> and providing the matching name.
    /// </para>
    /// <para>
    /// <typeparamref name="TClient"/> instances constructed with the appropriate <see cref="HttpClient" />
    /// can be retrieved from <see cref="IServiceProvider.GetService(Type)" /> (and related methods) by providing
    /// <typeparamref name="TClient"/> as the service type.
    /// </para>
    /// <para>
    /// Use <see cref="Options.DefaultName"/> as the name to configure the default client.
    /// </para>
    /// </remarks>
    public static IHttpClientBuilder AddKeeperHttpClient<TClient, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(
        this IServiceCollection services, string name, Action<HttpClient> configureClient)
        where TClient : class
        where TImplementation : class, TClient =>
        AddKeeperHttpClient(() => services.AddHttpClient<TClient, TImplementation>(name, configureClient));

    /// <summary>
    /// Adds the <see cref="IHttpClientFactory"/> and related services to the <see cref="IServiceCollection"/> and configures
    /// a binding between the <typeparamref name="TClient" /> type and a named <see cref="HttpClient"/>.
    /// Also enriches every HttpClient with the current Keeper correlation id if it is available.
    /// </summary>
    /// <typeparam name="TClient">
    /// The type of the typed client. The type specified will be registered in the service collection as
    /// a transient service. See <see cref="ITypedHttpClientFactory{TClient}" /> for more details about authoring typed clients.
    /// </typeparam>
    /// <typeparam name="TImplementation">
    /// The implementation type of the typed client. The type specified will be instantiated by the
    /// <see cref="ITypedHttpClientFactory{TImplementation}"/>
    /// </typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="name">The logical name of the <see cref="HttpClient"/> to configure.</param>
    /// <param name="configureClient">A delegate that is used to configure an <see cref="HttpClient"/>.</param>
    /// <returns>An <see cref="IHttpClientBuilder"/> that can be used to configure the client.</returns>
    /// <remarks>
    /// <para>
    /// <see cref="HttpClient"/> instances that apply the provided configuration can be retrieved using
    /// <see cref="IHttpClientFactory.CreateClient(string)"/> and providing the matching name.
    /// </para>
    /// <para>
    /// <typeparamref name="TClient"/> instances constructed with the appropriate <see cref="HttpClient" />
    /// can be retrieved from <see cref="IServiceProvider.GetService(Type)" /> (and related methods) by providing
    /// <typeparamref name="TClient"/> as the service type.
    /// </para>
    /// <para>
    /// Use <see cref="Options.DefaultName"/> as the name to configure the default client.
    /// </para>
    /// </remarks>
    public static IHttpClientBuilder AddKeeperHttpClient<TClient, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(
        this IServiceCollection services, string name, Action<IServiceProvider, HttpClient> configureClient)
        where TClient : class
        where TImplementation : class, TClient =>
        AddKeeperHttpClient(() => services.AddHttpClient<TClient, TImplementation>(name, configureClient));

#if NET5_0_OR_GREATER
    /// <summary>
    /// Adds the <see cref="IHttpClientFactory"/> and related services to the <see cref="IServiceCollection"/> and configures
    /// a binding between the <typeparamref name="TClient" /> type and a named <see cref="HttpClient"/>.
    /// Also enriches every HttpClient with the current Keeper correlation id if it is available.
    /// </summary>
    /// <typeparam name="TClient">
    /// The type of the typed client. The type specified will be registered in the service collection as
    /// a transient service. See <see cref="ITypedHttpClientFactory{TClient}" /> for more details about authoring typed clients.
    /// </typeparam>
    /// <typeparam name="TImplementation">
    /// The implementation type of the typed client.
    /// </typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="factory">A delegate that is used to create an instance of <typeparamref name="TClient"/>.</param>
    /// <returns>An <see cref="IHttpClientBuilder"/> that can be used to configure the client.</returns>
    /// <remarks>
    /// <para>
    /// <see cref="HttpClient"/> instances that apply the provided configuration can be retrieved using
    /// <see cref="IHttpClientFactory.CreateClient(string)"/> and providing the matching name.
    /// </para>
    /// <para>
    /// <typeparamref name="TClient"/> instances constructed with the appropriate <see cref="HttpClient" />
    /// can be retrieved from <see cref="IServiceProvider.GetService(Type)" /> (and related methods) by providing
    /// <typeparamref name="TClient"/> as the service type.
    /// </para>
    /// </remarks>
    public static IHttpClientBuilder AddKeeperHttpClient<TClient, TImplementation>(this IServiceCollection services, Func<HttpClient, TImplementation> factory)
        where TClient : class
        where TImplementation : class, TClient =>
        AddKeeperHttpClient(() => services.AddHttpClient<TClient, TImplementation>(factory));

    /// <summary>
    /// Adds the <see cref="IHttpClientFactory"/> and related services to the <see cref="IServiceCollection"/> and configures
    /// a binding between the <typeparamref name="TClient" /> type and a named <see cref="HttpClient"/>.
    /// Also enriches every HttpClient with the current Keeper correlation id if it is available.
    /// </summary>
    /// <typeparam name="TClient">
    /// The type of the typed client. The type specified will be registered in the service collection as
    /// a transient service. See <see cref="ITypedHttpClientFactory{TClient}" /> for more details about authoring typed clients.
    /// </typeparam>
    /// <typeparam name="TImplementation">
    /// The implementation type of the typed client.
    /// </typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="name">The logical name of the <see cref="HttpClient"/> to configure.</param>
    /// <param name="factory">A delegate that is used to create an instance of <typeparamref name="TClient"/>.</param>
    /// <returns>An <see cref="IHttpClientBuilder"/> that can be used to configure the client.</returns>
    /// <remarks>
    /// <para>
    /// <see cref="HttpClient"/> instances that apply the provided configuration can be retrieved using
    /// <see cref="IHttpClientFactory.CreateClient(string)"/> and providing the matching name.
    /// </para>
    /// <para>
    /// <typeparamref name="TClient"/> instances constructed with the appropriate <see cref="HttpClient" />
    /// can be retrieved from <see cref="IServiceProvider.GetService(Type)" /> (and related methods) by providing
    /// <typeparamref name="TClient"/> as the service type.
    /// </para>
    /// <typeparamref name="TImplementation">
    /// </typeparamref>
    /// </remarks>
    public static IHttpClientBuilder AddKeeperHttpClient<TClient, TImplementation>(this IServiceCollection services, string name, Func<HttpClient, TImplementation> factory)
        where TClient : class
        where TImplementation : class, TClient =>
        AddKeeperHttpClient(() => services.AddHttpClient<TClient, TImplementation>(name, factory));

    /// <summary>
    /// Adds the <see cref="IHttpClientFactory"/> and related services to the <see cref="IServiceCollection"/> and configures
    /// a binding between the <typeparamref name="TClient" /> type and a named <see cref="HttpClient"/>.
    /// Also enriches every HttpClient with the current Keeper correlation id if it is available.
    /// </summary>
    /// <typeparam name="TClient">
    /// The type of the typed client. The type specified will be registered in the service collection as
    /// a transient service. See <see cref="ITypedHttpClientFactory{TClient}" /> for more details about authoring typed clients.
    /// </typeparam>
    /// <typeparam name="TImplementation">
    /// The implementation type of the typed client.
    /// </typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="factory">A delegate that is used to create an instance of <typeparamref name="TClient"/>.</param>
    /// <returns>An <see cref="IHttpClientBuilder"/> that can be used to configure the client.</returns>
    /// <remarks>
    /// <para>
    /// <see cref="HttpClient"/> instances that apply the provided configuration can be retrieved using
    /// <see cref="IHttpClientFactory.CreateClient(string)"/> and providing the matching name.
    /// </para>
    /// <para>
    /// <typeparamref name="TClient"/> instances constructed with the appropriate <see cref="HttpClient" />
    /// can be retrieved from <see cref="IServiceProvider.GetService(Type)" /> (and related methods) by providing
    /// <typeparamref name="TClient"/> as the service type.
    /// </para>
    /// </remarks>
    public static IHttpClientBuilder AddKeeperHttpClient<TClient, TImplementation>(this IServiceCollection services, Func<HttpClient, IServiceProvider, TImplementation> factory)
        where TClient : class
        where TImplementation : class, TClient =>
        AddKeeperHttpClient(() => services.AddHttpClient<TClient, TImplementation>(factory));

    /// <summary>
    /// Adds the <see cref="IHttpClientFactory"/> and related services to the <see cref="IServiceCollection"/> and configures
    /// a binding between the <typeparamref name="TClient" /> type and a named <see cref="HttpClient"/>.
    /// Also enriches every HttpClient with the current Keeper correlation id if it is available.
    /// </summary>
    /// <typeparam name="TClient">
    /// The type of the typed client. The type specified will be registered in the service collection as
    /// a transient service. See <see cref="ITypedHttpClientFactory{TClient}" /> for more details about authoring typed clients.
    /// </typeparam>
    /// <typeparam name="TImplementation">
    /// The implementation type of the typed client.
    /// </typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="name">The logical name of the <see cref="HttpClient"/> to configure.</param>
    /// <param name="factory">A delegate that is used to create an instance of <typeparamref name="TClient"/>.</param>
    /// <returns>An <see cref="IHttpClientBuilder"/> that can be used to configure the client.</returns>
    /// <remarks>
    /// <para>
    /// <see cref="HttpClient"/> instances that apply the provided configuration can be retrieved using
    /// <see cref="IHttpClientFactory.CreateClient(string)"/> and providing the matching name.
    /// </para>
    /// <para>
    /// <typeparamref name="TClient"/> instances constructed with the appropriate <see cref="HttpClient" />
    /// can be retrieved from <see cref="IServiceProvider.GetService(Type)" /> (and related methods) by providing
    /// <typeparamref name="TClient"/> as the service type.
    /// </para>
    /// </remarks>
    public static IHttpClientBuilder AddKeeperHttpClient<TClient, TImplementation>(this IServiceCollection services, string name, Func<HttpClient, IServiceProvider, TImplementation> factory)
        where TClient : class
        where TImplementation : class, TClient =>
        AddKeeperHttpClient(() => services.AddHttpClient<TClient, TImplementation>(name, factory));
#endif
    private static IHttpClientBuilder AddKeeperHttpClient(Func<IHttpClientBuilder> getBuilder) =>
        getBuilder()
        .ConfigureHttpClient(SetCurrentHttpClientCorrelationId);

    private static void SetCurrentHttpClientCorrelationId(HttpClient httpClient)
    {
        var applicationState = KeeperApplicationContext.GetCurrentApplicationState();
        if (applicationState != null)
        {
            httpClient.SetApplicationStateHeaders(applicationState);
        }
    }

    /// <summary>
    /// Executes the call.
    /// </summary>
    /// <typeparam name="TResponseData">The type of the expected response.</typeparam>
    /// <typeparam name="TRequestData">The type of the request</typeparam>
    /// <param name="clientFactory">The client factory.</param>
    /// <param name="request">The request.</param>
    /// <param name="clientName">The client name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The rest response.</returns>
    public static async Task<IRestResponse<TResponseData, TRequestData>> ExecuteAsync<TResponseData, TRequestData>(
        this IHttpClientFactory clientFactory,
        IRestRequest<TRequestData> request,
        string? clientName = null,
        CancellationToken cancellationToken = default)
    {
        using var client = clientFactory.CreateClient(clientName ?? string.Empty);

        return await client
            .ExecuteAsyncInternal<RestResponse<TResponseData, TRequestData>>(request, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Executes the call.
    /// </summary>
    /// <param name="clientFactory">The client factory.</param>
    /// <param name="request">The request.</param>
    /// <param name="clientName">The client name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The rest response.</returns>
    public static async Task<IRestResponse> ExecuteAsync(
        this IHttpClientFactory clientFactory,
        IRestRequest request,
        string? clientName = null,
        CancellationToken cancellationToken = default)
    {
        using var client = clientFactory.CreateClient(clientName ?? string.Empty);

        return await client
            .ExecuteAsyncInternal<RestResponse>(request, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Executes the call.
    /// </summary>
    /// <typeparam name="TResponseData">The type of the expected response.</typeparam>
    /// <param name="clientFactory">The client factory.</param>
    /// <param name="request">The request.</param>
    /// <param name="clientName">The client name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The rest response.</returns>
    public static async Task<IRestResponse<TResponseData>> ExecuteAsync<TResponseData>(this IHttpClientFactory clientFactory, IRestRequest request, string? clientName = null, CancellationToken cancellationToken = default)
    {
        using var client = clientFactory.CreateClient(clientName ?? string.Empty);

        return await client
            .ExecuteAsyncInternal<RestResponse<TResponseData>>(request, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Executes the call.
    /// </summary>
    /// <typeparam name="TResponseData">The type of the expected response.</typeparam>
    /// <typeparam name="TRequestData">The type of the request</typeparam>
    /// <param name="client">The http client.</param>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The rest response.</returns>
    public static async Task<IRestResponse<TResponseData, TRequestData>> ExecuteAsync<TResponseData, TRequestData>(this HttpClient client, IRestRequest<TRequestData> request, CancellationToken cancellationToken = default)
    {
        return await client.ExecuteAsyncInternal<RestResponse<TResponseData, TRequestData>>(request, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes the call.
    /// </summary>
    /// <param name="client">The http client.</param>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The rest response.</returns>

    public static async Task<IRestResponse> ExecuteAsync(this HttpClient client, IRestRequest request, CancellationToken cancellationToken = default)
    {
        return await client.ExecuteAsyncInternal<RestResponse>(request, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes the call.
    /// </summary>
    /// <typeparam name="TResponseData">The type of the expected response.</typeparam>
    /// <param name="client">The http client.</param>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The rest response.</returns>
    public static async Task<IRestResponse<TResponseData>> ExecuteAsync<TResponseData>(this HttpClient client, IRestRequest request, CancellationToken cancellationToken = default)
    {
        return await client.ExecuteAsyncInternal<RestResponse<TResponseData>>(request, cancellationToken).ConfigureAwait(false);
    }

    private static async Task<TResponseType> ExecuteAsyncInternal<TResponseType>(this HttpClient client, IRestRequest request, CancellationToken cancellationToken = default)
        where TResponseType : RestResponse, new()
    {
        TResponseType result = default!;
        HttpResponseMessage response = null!;
        var httpRequest = request.ToHttpRequestMessage();

        try
        {
            response = await client.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                throw new KeeperHttpRequestException($"Response status code does not indicate success: {response.StatusCode}");

            result = await response.ToRestResponse<TResponseType>(request).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            result = await e.ToRestResponse<TResponseType>(response, request).ConfigureAwait(false);
        }
        finally
        {
            if (result != null && result.Request is RestRequest restRequest)
                restRequest.RequestUri = httpRequest.RequestUri!;

            foreach (var header in client.DefaultRequestHeaders)
                foreach (var value in header.Value)
                    request.Headers.Add(header.Key, value);
        }
        return result;
    }

    private static async Task<TResponseType> ToRestResponse<TResponseType>(this HttpResponseMessage httpResponseMessage, IRestRequest? request = null, Exception? exception = null)
        where TResponseType : RestResponse, new()
    {
        byte[] rawBytes = await httpResponseMessage.Content.SafeReadAsByteArrayAsync().ConfigureAwait(false);
        var contentLength = rawBytes.Length;
        string? contentType = httpResponseMessage.Content?.Headers?.ContentType?.MediaType;
        var headers = new NameValueCollection();

        foreach (var header in httpResponseMessage.Headers)
        {
            headers.Add(header.Key, string.Join(";", header.Value));
        }

        var restResponse = new TResponseType
        {
            RawBytes = rawBytes,
            ContentLength = contentLength,
            ContentType = contentType,
            Headers = headers,
            StatusCode = httpResponseMessage.StatusCode,
            StatusDescription = httpResponseMessage.ReasonPhrase,
            IsSuccessful = httpResponseMessage.IsSuccessStatusCode,
            Request = request,
            ErrorException = exception,
            ErrorMessage = exception?.Message
        };

        if (restResponse is IRestResponseWithSerializer restResponseWithSerializer)
        {
            try
            {
                restResponseWithSerializer.DeserializeBody();
            }
            catch (Exception e)
            {
                restResponse.ErrorException = exception == null ? e : new AggregateException(exception, e);
            }
            finally
            {
                restResponse.ErrorMessage = restResponse.ErrorException?.Message;
            }
        }

        return restResponse;
    }

    private static async Task<TResponseType> ToRestResponse<TResponseType>(this Exception exception, HttpResponseMessage httpResponseMessage, IRestRequest? request = null)
        where TResponseType : RestResponse, new()
    {
        byte[] rawBytes = Array.Empty<byte>();
        string? contentType = null;
        NameValueCollection? headers = null;
        if (httpResponseMessage != null)
        {
            rawBytes = await httpResponseMessage.Content.SafeReadAsByteArrayAsync().ConfigureAwait(false);
            contentType = httpResponseMessage.Content?.Headers?.ContentType?.MediaType;
            headers = new NameValueCollection();
            foreach (var header in httpResponseMessage.Headers)
            {
                headers.Add(header.Key, string.Join(";", header.Value));
            }
        }

        return new TResponseType
        {
            RawBytes = rawBytes,
            ContentLength = rawBytes.Length,
            ContentType = contentType,
            Headers = headers,
            StatusCode = httpResponseMessage?.StatusCode,
            StatusDescription = httpResponseMessage?.ReasonPhrase,
            IsSuccessful = httpResponseMessage?.IsSuccessStatusCode ?? false,
            Request = request,
            ErrorException = exception,
            ErrorMessage = exception.Message
        };
    }

    private static HttpRequestMessage ToHttpRequestMessage(this IRestRequest request)
    {
        var httpRequest = new HttpRequestMessage(request.HttpMethod, new Uri(request.Resource, UriKind.RelativeOrAbsolute));

        if (request.Content != null)
        {
            httpRequest.Content = new ByteArrayContent(request.Content);
        }

        bool isContainsContentType = false;
        if (request.Headers?.AllKeys is not null)
        {
            foreach (var key in request.Headers.AllKeys)
            {
                if (httpRequest.Content != null &&
                    key!.Equals(CONTENT_TYPE_HEADER, StringComparison.CurrentCultureIgnoreCase))
                {
                    httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue(request.Headers[key]!);
                    isContainsContentType = true;
                }
                else
                    httpRequest.Headers.Add(key!, request.Headers.GetValues(key)!);
            }
        }

        if (httpRequest.Content != null && !isContainsContentType)
            httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue(DEFAULT_CONTENT_TYPE);

        if (request.AuthenticationHeaderValue is not null)
            httpRequest.Headers.Authorization = request.AuthenticationHeaderValue;

        return httpRequest;
    }

    /// <summary>
    /// Reads byte array safely from http content.
    /// </summary>
    /// <param name="httpContent">The http content.</param>
    /// <returns>The byte array.</returns>
    public static async Task<byte[]> SafeReadAsByteArrayAsync(this HttpContent httpContent)
    {
        try
        {
            if (httpContent != null)
                return await httpContent.ReadAsByteArrayAsync().ConfigureAwait(false);
            return Array.Empty<byte>();
        }
        catch (ObjectDisposedException)
        {
            return Array.Empty<byte>();
        }
    }

    /// <summary>
    /// Converts <see cref="Nullable{HttpStatusCode}"/> to an integer.
    /// </summary>
    /// <param name="code">The status code.</param>
    /// <returns>The integer value of the status code or null.</returns>
    public static int? ToInt32(this HttpStatusCode? code) => code?.ToInt32();

    /// <summary>
    /// Converts <see cref="HttpStatusCode"/> to an integer.
    /// </summary>
    /// <param name="code">The status code.</param>
    /// <returns>The integer value of the status code.</returns>
    public static int ToInt32(this HttpStatusCode code) => (int)code;

    /// <summary>
    /// Sets the Keeper headers on an httpclient from an application state.
    /// </summary>
    /// <param name="httpClient">The http client.</param>
    /// <param name="applicationState">The application state.</param>
    public static void SetApplicationStateHeaders(this HttpClient httpClient, IApplicationState applicationState)
    {
        httpClient.SetKeeperCorrelationId(applicationState.CorrelationId);
        if (!string.IsNullOrWhiteSpace(applicationState.IdempotencyKey))
            httpClient.SetIdempotencyKey(applicationState.IdempotencyKey);
    }

    /// <summary>
    /// Sets the Keeper correlation id header on an httpclient from an application state.
    /// </summary>
    /// <param name="httpClient">The http client.</param>
    /// <param name="applicationState">The application state.</param>
    public static void SetKeeperCorrelationId(this HttpClient httpClient, IApplicationState applicationState) =>
        httpClient.SetKeeperCorrelationId(applicationState.CorrelationId);

    /// <summary>
    /// Sets the Keeper correlation id header on an httpclient.
    /// </summary>
    /// <param name="httpClient">The http client.</param>
    /// <param name="correlationId">The correlation id.</param>
    public static void SetKeeperCorrelationId(this HttpClient httpClient, Guid correlationId) =>
        httpClient.SetKeeperCorrelationId(correlationId.ToString());

    /// <summary>
    /// Sets the Keeper correlation id header on an httpclient.
    /// </summary>
    /// <param name="httpClient">The http client.</param>
    /// <param name="correlationId">The correlation id.</param>
    public static void SetKeeperCorrelationId(this HttpClient httpClient, string correlationId) =>
        httpClient.DefaultRequestHeaders.Add(Headers.KeeperCorrelationId, correlationId);

    /// <summary>
    /// Sets the Keeper idempotency key id header on an httpclient from an application state.
    /// </summary>
    /// <param name="httpClient">The http client.</param>
    /// <param name="applicationState">The application state.</param>
    public static void SetIdempotencyKey(this HttpClient httpClient, IApplicationState applicationState) =>
        httpClient.SetIdempotencyKey(applicationState.IdempotencyKey);

    /// <summary>
    /// Sets the Keeper idempotency key header on an httpclient.
    /// </summary>
    /// <param name="httpClient">The http client.</param>
    /// <param name="idempotencyKey">The idempotency key.</param>
    public static void SetIdempotencyKey(this HttpClient httpClient, Guid idempotencyKey) =>
        httpClient.SetIdempotencyKey(idempotencyKey.ToString());

    /// <summary>
    /// Sets the Keeper idempotency key header on an httpclient.
    /// </summary>
    /// <param name="httpClient">The http client.</param>
    /// <param name="idempotencyKey">The idempotency key.</param>
    public static void SetIdempotencyKey(this HttpClient httpClient, string idempotencyKey) =>
        httpClient.DefaultRequestHeaders.Add(Headers.IdempotencyKey, idempotencyKey);
}