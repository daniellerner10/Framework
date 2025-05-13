using System;
using System.Collections.Generic;

namespace Keeper.Framework.Extensions.Reflection
{
    public class MethodCallModel
    {
        public required string? AssemblyQualifiedName { get; set; }

        public required string ClassName {  get; set; }

        public required string MethodName { get; set; }

        public required List<object?> Arguments { get; set; }

        public required Type ReturnValue { get; set; }
    }
}
