using Npgsql;
using SQL_Studio.Services.Interfaces;
using System.Text;

namespace SQL_Studio.Services
{
    public class SchemaProvider : ISchemaProvider
    {
        public async Task<string> GetSchemaTextAsync(IConnectionFactoryService connectionFactory, string databaseName,
            CancellationToken cancellationToken)
        {
            await using var connection = await connectionFactory.OpenConnectionAsync(databaseName);

            var columns = new Dictionary<string, List<string>>();
            const string columnsQuery = """
                SELECT table_name, column_name, data_type
                FROM information_schema.columns
                WHERE table_schema = 'public'
                ORDER BY table_name, ordinal_position
                """;
            await using (var command = new NpgsqlCommand(columnsQuery, connection))
            await using (var reader = await command.ExecuteReaderAsync(cancellationToken))
            {
                while (await reader.ReadAsync(cancellationToken))
                {
                    string table = reader.GetString(0);
                    if (!columns.TryGetValue(table, out var list))
                    {
                        list = new List<string>();
                        columns[table] = list;
                    }
                    list.Add($"{reader.GetString(1)} {reader.GetString(2)}");
                }
            }

            var constraints = new Dictionary<string, List<string>>();
            const string constraintsQuery = """
                SELECT conrelid::regclass::text, pg_get_constraintdef(oid)
                FROM pg_constraint
                WHERE connamespace = 'public'::regnamespace AND contype IN ('p', 'f')
                ORDER BY conrelid::regclass::text, contype
                """;
            await using (var command = new NpgsqlCommand(constraintsQuery, connection))
            await using (var reader = await command.ExecuteReaderAsync(cancellationToken))
            {
                while (await reader.ReadAsync(cancellationToken))
                {
                    string table = reader.GetString(0);
                    if (!constraints.TryGetValue(table, out var list))
                    {
                        list = new List<string>();
                        constraints[table] = list;
                    }
                    list.Add(reader.GetString(1));
                }
            }

            var schema = new StringBuilder();
            foreach (var (table, tableColumns) in columns)
            {
                schema.AppendLine($"TABLE {table} ({string.Join(", ", tableColumns)})");
                if (constraints.TryGetValue(table, out var tableConstraints))
                {
                    foreach (var constraint in tableConstraints)
                    {
                        schema.AppendLine($"  {constraint}");
                    }
                }
            }
            return schema.ToString();
        }
    }
}
