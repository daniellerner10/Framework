using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Keeper.Framework.Extensions.Reflection;

internal class AssemblyScanner
{
    private readonly string _directory;

    public AssemblyScanner(string directory)
    {
        _directory = directory;
    }

    public IEnumerable<Type> GetTypes() =>
        GetAssemblies()
        .SelectMany(GetTypesFromAssembly!);

    public IEnumerable<Assembly?> GetAssemblies() =>
        GetFilesToScan()
        .Select(FileToAssembly)
        .Where(static a => a is not null);

    private Assembly? FileToAssembly(string file) =>
        TryLoadAssembly(file, out var assembly) ?
            assembly : null;

    private string[] GetFilesToScan() =>
         Directory.GetFiles(_directory, "*.dll");

    private static bool TryLoadAssembly(string path, out Assembly assembly)
    {
        try
        {
            assembly = Assembly.LoadFrom(path);
            return true;
        }
        catch (Exception)
        {
            assembly = default!;
            return false;
        }
    }

    private static IEnumerable<Type> GetTypesFromAssembly(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (Exception)
        {
            return [];
        }
    }
}
