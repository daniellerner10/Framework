using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Keeper.Framework.Extensions.Data
{
    public static class PostgresFunctionExtensions
    {
        [PgMethod("coalesce", "any")]
        public static T Coalesce<T>(this DbFunctions _, params T?[] input) =>
            throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Coalesce)));

        [PgMethod("jsonb_build_array", "jsonb")]
        public static string JsonbBuildArray(this DbFunctions _, params object?[] input) =>
            throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(JsonbBuildArray)));

        [PgMethod("jsonb_build_object", "jsonb")]
        public static string JsonbBuildObject(this DbFunctions _, params object?[] input) =>
            throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(JsonbBuildObject)));

        [PgMethod("merge_action", "text")]
        public static MergeType GetMergeType<TUpdateEntity>(this TUpdateEntity entity) where TUpdateEntity : class =>
                throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(GetMergeType)));
    }
}
