using Serilog.Context;
using Serilog.Enrichers.Sensitive;

namespace Keeper.Framework.Logging;

/// <summary>
 /// Static class used for adding custom properties to a logging context.  Use as follows:
 /// <code>
 /// using (LoggerContext.PushProperty("MyCustomProperty", "MyCustomValue"))
 /// {
 ///    logger.Information("My message");
 /// }
 /// </code>
 /// </summary>
public static class LoggerContext
{
    /// <summary>
    /// Push a property onto the context, returning an System.IDisposable that must later
    /// be used if you want to remove the property. The property must be popped from the same 
    /// thread/logical call context.
    /// 
    /// Use as follows:
    /// <code>
    /// using (LoggerContext.PushProperty("MyCustomProperty", "MyCustomValue"))
    /// {
    ///    logger.Information("My message");
    /// }
    /// </code>
    /// </summary>
    /// <param name="name">The name of the property.</param>
    /// <param name="value">The value of the property.</param>
    /// <param name="destructureObjects">
    /// If true, and the value is a non-primitive, non-array type, then the value will
    /// be converted to a structure; otherwise, unknown types will be converted to scalars,
    /// which are generally stored as strings.
    /// </param>
    /// <returns>A handle to later remove the property from the context.</returns>
    public static IDisposable PushProperty(string name, object value, bool destructureObjects = false) =>
        LogContext.PushProperty(name, value, destructureObjects);

    /// <summary>
    /// Push properties onto the context, returning an System.IDisposable that must later
    /// be used if you want to remove the property. The properties must be popped from the same 
    /// thread/logical call context.
    /// 
    /// Use as follows:
    /// <code>
    /// using (LoggerContext.PushProperties(myPropertiesDictionary))
    /// {
    ///    logger.Information("My message");
    /// }
    /// </code>        
    /// </summary>
    /// <param name="properties">The dictionary of properties.</param>
    /// <returns>A handle to later remove the property from the context.</returns>
    public static IDisposable PushProperties(Dictionary<string, string> properties) =>
        LogContext.Push(new PropertyEnricher(properties));

    public static IDisposable LoggerEnterSensitiveArea()
    {
        return Serilog.Log.Logger.EnterSensitiveArea();
    }
}