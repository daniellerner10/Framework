using Npgsql;

namespace Keeper.Framework.Extensions.Data
{
    internal class NpgSqlBinaryWriter(NpgsqlBinaryImporter _npgsqlBinaryImporter) : IBinaryWriter
    {
        public Task WriteRowAsync(CancellationToken cancellationToken, object[] values) =>
            _npgsqlBinaryImporter.WriteRowAsync(cancellationToken, values);

        public ValueTask<ulong> CompleteAsync(CancellationToken cancellationToken) =>
            _npgsqlBinaryImporter.CompleteAsync(cancellationToken);

        public async ValueTask DisposeAsync()
        {
            await _npgsqlBinaryImporter.DisposeAsync();
        }
    }
}
