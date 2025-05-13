namespace Keeper.Framework.Extensions.Data;

[AttributeUsage(AttributeTargets.Method)]
public class PgMethodAttribute(string methodName, string returnType) : Attribute
{
    public string MethodName => methodName;

    public string ReturnType => returnType;
}
