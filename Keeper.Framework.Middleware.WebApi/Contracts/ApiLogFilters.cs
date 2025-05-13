namespace Keeper.Framework.Middleware.WebApi.Contracts
{
    public class ApiLogFilters
    {
        public List<string> ApiLogRequestFilters { get; set; } = [];

        public List<string> ApiLogResponseFilters { get; set; } = [];

        public List<string> ApiLogRequestResponseFilters { get; set; } = [];
    }
}
