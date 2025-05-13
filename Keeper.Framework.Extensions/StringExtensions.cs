using System.Runtime.CompilerServices;
using System.Text;

namespace Keeper.Framework.Extensions;

/// <summary>
/// Extensions for string.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Return as string as ReadOnlyMemory.
    /// </summary>
    /// <param name="str">The string.</param>
    /// <param name="encoding">The encoding.  Defaults to UTF8.</param>
    /// <returns>string as ReadOnlyMemory.</returns>
    public static ReadOnlyMemory<byte> AsReadOnlyMemory(this string str, Encoding? encoding = null)
    {
        if (str is null)
            return null;

        encoding ??= Encoding.UTF8;

        return new ReadOnlyMemory<byte>(encoding.GetBytes(str));
    }

    /// <summary>
    /// If the string contains any of the chars in the array, it returns true.
    /// </summary>
    /// <param name="str"></param>
    /// <param name="chars"></param>
    /// <returns></returns>
    public static bool ContainsAny(this string str, IEnumerable<char> chars)
    {
        if (str == null) return false;
        return str.IndexOfAny(chars.ToArray()) >= 0;
    }

    /// <summary>
    /// Substring that wont throw an exception if the bounds are broken (0,9999).
    /// Substring without length that wont throw an exception if the bounds are broken (0)
    /// If string null is inputed, null is returned.
    /// </summary>
    public static string SafeSubstring(this string str, int startIndex, int length)
    {
        if (startIndex < 0)
            startIndex = 0;

        if (startIndex >= str.Length)
            startIndex = str.Length;

        if (length < 0)
            length = 0;

        if ((startIndex + length) >= str.Length)
            length = str.Length - startIndex;

        return str.Substring(startIndex, length);
    }

    /// <summary>
    /// Substring that wont throw an exception if the bounds are broken (0,9999).
    /// Substring without length that wont throw an exception if the bounds are broken (0)
    /// If string null is inputed, null is returned.
    /// </summary>
    public static string SafeSubstring(this string str, int startIndex)
    {
        return str.SafeSubstring(startIndex, str.Length - startIndex);
    }

    /// <summary>
    /// Converts a string to camel case.
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string ToCamelCase(this string str)
    {
        if (!string.IsNullOrWhiteSpace(str))
            return $"{str.First().ToString().ToLowerInvariant()}{str.SafeSubstring(1)}";

        return str;
    }

    /// <summary>
    /// Asserts whether the string is null.  If it is throws an exception.
    /// </summary>
    /// <param name="str">The string.</param>
    /// <param name="variableName">The variable name to describe in the exception. DO NOT pass
    /// this variable unless you are targeting net461. In other targets, the compiler will fill
    /// the value in automatically.</param>
    /// <returns>The string.</returns>
    /// <exception cref="ArgumentNullException">Thrown if str is null.</exception>
    public static string AssertIsNotNull(this string str, [CallerArgumentExpression("str")] string? variableName = null) =>
        str ?? throw new ArgumentNullException(variableName);

    /// <summary>
    /// Asserts whether the string is null or empty.  If it is throws an exception.
    /// </summary>
    /// <param name="str">The string.</param>
    /// <param name="variableName">The variable name to describe in the exception. DO NOT pass
    /// this variable unless you are targeting net461. In other targets, the compiler will fill
    /// the value in automatically.</param>
    /// <returns>The string.</returns>
    /// <exception cref="ArgumentNullException">Thrown if str is null or empty.</exception>
    public static string AssertIsNotNullOrEmpty(this string str, [CallerArgumentExpression("str")] string? variableName = null) =>
        string.IsNullOrEmpty(str) ?
            throw new ArgumentNullException(variableName) :
            str;

    /// <summary>
    /// Asserts whether the string is null or whitespace.  If it is throws an exception.
    /// </summary>
    /// <param name="str">The string.</param>
    /// <param name="variableName">The variable name to describe in the exception. DO NOT pass
    /// this variable unless you are targeting net461. In other targets, the compiler will fill
    /// the value in automatically.</param>
    /// <returns>The string.</returns>
    /// <exception cref="ArgumentNullException">Thrown if str is null or whitespace.</exception>
    public static string AssertIsNotNullOrWhiteSpace(this string str, [CallerArgumentExpression("str")] string? variableName = null) =>
        string.IsNullOrWhiteSpace(str) ?
            throw new ArgumentNullException(variableName) :
            str;
}