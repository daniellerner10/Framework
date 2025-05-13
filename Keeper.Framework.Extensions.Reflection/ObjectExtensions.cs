using System;
using System.Linq;
using System.Reflection;

namespace Keeper.Framework.Extensions.Reflection;

public static class ObjectExtensions
{
    public static T GetNonPublicPropertyValue<T>(this object obj, string propertyName)
    {
        ArgumentNullException.ThrowIfNull(obj);
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        var pi = obj.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.NonPublic);
        return (T)pi?.GetValue(obj)! ?? throw new MissingMemberException(typeof(T).FullName, propertyName);
    }

    public static T GetNonPublicFieldValue<T>(this object obj, string fieldName)
    {
        ArgumentNullException.ThrowIfNull(obj);
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldName);

        var fi = obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        return (T)fi?.GetValue(obj)! ?? throw new MissingFieldException(typeof(T).FullName, fieldName);
    }

    public static T CallNonPublicMethod<T>(this object obj, string methodName, params object[] args)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(methodName);

        var mi = obj.GetMethodInfo(mi => mi.Name == methodName)
            ?? throw new MissingMethodException(typeof(T).FullName, methodName);

        return (T)mi.Invoke(obj, args)!;
    }

    public static T CallNonPublicMethod<T>(this object obj, Func<MethodInfo, bool> methodPredicate, params object[] args)
    {
        ArgumentNullException.ThrowIfNull(obj);
        ArgumentNullException.ThrowIfNull(methodPredicate);

        var mi = obj.GetMethodInfo(methodPredicate)
            ?? throw new MissingMethodException("Method not found");

        return (T)mi.Invoke(obj, args)!;
    }

    private static MethodInfo? GetMethodInfo(this object obj, Func<MethodInfo, bool> methodPredicate) =>
        obj.GetType()
           .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
           .SingleOrDefault(methodPredicate);
}
