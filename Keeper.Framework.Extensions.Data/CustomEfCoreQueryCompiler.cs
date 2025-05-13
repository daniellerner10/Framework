using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Keeper.Framework.Extensions.Data
{
    [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "Is is necessary.")]
    [SuppressMessage("Usage", "EF1001:Internal EF Core API usage.", Justification = "We know. we wish it was public but it is not.")]
    internal class CustomEfCoreQueryCompiler(
        IQueryContextFactory queryContextFactory,
        ICompiledQueryCache compiledQueryCache,
        ICompiledQueryCacheKeyGenerator compiledQueryCacheKeyGenerator,
        IDatabase database,
        IDiagnosticsLogger<Query> logger,
        ICurrentDbContext currentContext,
        IEvaluatableExpressionFilter evaluatableExpressionFilter,
        IModel model) : QueryCompiler(queryContextFactory,
                                      compiledQueryCache,
                                      compiledQueryCacheKeyGenerator,
                                      database,
                                      logger,
                                      currentContext,
                                      evaluatableExpressionFilter,
                                      model)
    {
        public override Expression ExtractParameters(
            Expression query,
            IParameterValues parameterValues,
            IDiagnosticsLogger<Query> logger,
            bool parameterize = true,
            bool generateContextAccessors = false) =>
                base.ExtractParameters(
                    query, 
                    parameterValues, 
                    logger, 
                    parameterize: false, 
                    generateContextAccessors
                );

        public static string ToQueryString(DbContext context, IQueryable source)
        {
            var infrastructure = context.GetInfrastructure();
            var compiler = (CustomEfCoreQueryCompiler)ActivatorUtilities.CreateInstance(infrastructure, typeof(CustomEfCoreQueryCompiler));
            var entityQueryProvider = new EntityQueryProvider(compiler);
            var queryingEnumerable = (IQueryingEnumerable)entityQueryProvider.Execute<IEnumerable>(source.Expression);

            return queryingEnumerable.ToQueryString();
        }
    }
}