namespace Keeper.Framework.Middleware.Idempotency;

internal static class DatabaseConstants
{
    public const string IdempotencySchemaName = "idempotency";
    public const string DefaultIdempotencyTableName = "Keys";
    public const int SqlServerDuplicateKeyConstraintViolation = 2627;
    public const string PostgresDuplicateKeyConstraintViolation = "23505";
}