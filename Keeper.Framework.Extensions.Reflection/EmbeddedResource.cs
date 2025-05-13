using Keeper.Framework.Extensions.Streams;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Keeper.Framework.Extensions.Reflection;

/// <summary>
/// Helper for embedded resources.
/// </summary>
public static class EmbeddedResource
{
    /// <summary>
    /// Get embedded resource as stream.
    /// </summary>
    /// <param name="resourceName">The name of the resource</param>
    /// <param name="assembly">The assembly containing the embedded resource. If null, then the calling assembly.</param>
    /// <returns>The stream.<</returns>
    public static Stream GetStream(string resourceName, Assembly assembly = default!)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceName);

        assembly ??= Assembly.GetCallingAssembly();

        var assemblyName = assembly.GetName().Name;
        return assembly.GetManifestResourceStream($"{assemblyName}.{resourceName}")!;
    }

    /// <summary>
    /// Get embedded resource as string.
    /// </summary>
    /// <param name="resourceName">The name of the resource.</param>
    /// <param name="assembly">The assembly containing the embedded resource. If null, then the calling assembly.</param>
    /// <param name="encoding">The encoding to use.  Defaults to UTF8.</param>
    /// <returns>The string.</returns>
    public static string GetString(string resourceName, Assembly assembly = default!, Encoding encoding = default!)
    {
        encoding ??= Encoding.UTF8;
        assembly ??= Assembly.GetCallingAssembly();

        using var stream = GetStream(resourceName, assembly);
        using var reader = new StreamReader(stream, encoding);

        return reader.ReadToEnd();
    }

    /// <summary>
    /// Get embedded resource as bytes.
    /// </summary>
    /// <param name="resourceName">The name of the resource.</param>
    /// <param name="assembly">The assembly containing the embedded resource. If null, then the calling assembly.</param>
    /// <returns>The bytes.</returns>
    public static async Task<byte[]> GetBytesAsync(string resourceName, Assembly assembly = default!)
    {
        assembly ??= Assembly.GetCallingAssembly();

        using var stream = GetStream(resourceName, assembly);

        return await stream.ReadToEndAsync();
    }

    public static byte[] GetBytes(string resourceName, Assembly assembly = default!)
    {
        assembly ??= Assembly.GetCallingAssembly();

        using var stream = GetStream(resourceName, assembly);

        return stream.ReadAllBytes();
    }
}
