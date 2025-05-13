using System;
using System.Collections.Generic;
using System.Reflection;

namespace Keeper.Framework.Extensions.Reflection;

public static class AssemblyScan
{
    private static AssemblyScanner _scanner = default!;
    private static AssemblyScanner Scanner => _scanner ??= new(AppContext.BaseDirectory);

    public static IEnumerable<Type> Types => Scanner.GetTypes();

    public static IEnumerable<Assembly> Assemblies => Scanner.GetAssemblies()!;
}
