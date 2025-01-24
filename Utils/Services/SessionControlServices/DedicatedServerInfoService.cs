using MySql.Data.MySqlClient;

public class DedicatedServerInfoService
{
    private readonly string _connectionString;

    public DedicatedServerInfoService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task AddDedicatedServerAsync(string serverId, string serverAddress, string transportAddress, int port)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        string deleteSql = @"DELETE FROM dedicatedserver LIMIT 1";
        using var deleteCommand = new MySqlCommand(deleteSql, connection);
        await deleteCommand.ExecuteNonQueryAsync();

        string insertSql = @"
        INSERT INTO dedicatedserver 
        (ServerId, ServerAddress, TransportAddress, Port) 
        VALUES (@ServerId, @ServerAddress, @TransportAddress, @Port)";

        using var insertCommand = new MySqlCommand(insertSql, connection);
        insertCommand.Parameters.AddWithValue("@ServerId", serverId);
        insertCommand.Parameters.AddWithValue("@ServerAddress", serverAddress);
        insertCommand.Parameters.AddWithValue("@TransportAddress", transportAddress);
        insertCommand.Parameters.AddWithValue("@Port", port);

        await insertCommand.ExecuteNonQueryAsync();
    }

    public async Task<(string ServerId, string ServerAddress, string TransportAddress, int Port)> GetDedicatedServerInfoAsync()
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        string selectSql = "SELECT ServerId, ServerAddress, TransportAddress, Port FROM dedicatedserver LIMIT 1";
        using var selectCommand = new MySqlCommand(selectSql, connection);
        using var reader = await selectCommand.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            var serverId = reader.GetString(reader.GetOrdinal("ServerId"));
            var serverAddress = reader.GetString(reader.GetOrdinal("ServerAddress"));
            var transportAddress = reader.GetString(reader.GetOrdinal("TransportAddress"));
            var port = reader.GetInt32(reader.GetOrdinal("Port"));

            return (serverId, serverAddress, transportAddress, port);
        }
        return (null, null, null, 0);
    }
}