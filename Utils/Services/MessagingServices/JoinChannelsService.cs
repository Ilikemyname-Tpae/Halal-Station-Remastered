using MySql.Data.MySqlClient;
using System.Data;
using System.Text.Json;

namespace Halal_Station_Remastered.Utils.Services.MessagingServices
{
    public class JoinChannelsService
    {
        private readonly string _connectionString;

        public JoinChannelsService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task AddUserToChannelsAsync(int? userId, IEnumerable<string> channelNames)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            using var transaction = await connection.BeginTransactionAsync();

            foreach (var channelName in channelNames)
            {
                var selectQuery = "SELECT Id, Members FROM Channels WHERE Name = @Name";
                using var selectCommand = new MySqlCommand(selectQuery, connection, transaction);
                selectCommand.Parameters.AddWithValue("@Name", channelName);

                using var reader = await selectCommand.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var channelId = reader.GetInt32("Id");
                    var existingMembers = reader.GetString("Members");

                    var membersList = JsonSerializer.Deserialize<List<int?>>(existingMembers) ?? new List<int?>();

                    if (!membersList.Contains(userId))
                    {
                        membersList.Add(userId);
                    }

                    reader.Close();

                    var updateQuery = "UPDATE Channels SET Members = @Members WHERE Id = @Id";
                    using var updateCommand = new MySqlCommand(updateQuery, connection, transaction);
                    updateCommand.Parameters.AddWithValue("@Members", JsonSerializer.Serialize(membersList));
                    updateCommand.Parameters.AddWithValue("@Id", channelId);
                    await updateCommand.ExecuteNonQueryAsync();
                }
                else
                {
                    reader.Close();

                    var insertQuery = "INSERT INTO Channels (Name, Messages, Members) VALUES (@Name, @Messages, @Members)";
                    using var insertCommand = new MySqlCommand(insertQuery, connection, transaction);
                    insertCommand.Parameters.AddWithValue("@Name", channelName);
                    insertCommand.Parameters.AddWithValue("@Messages", "[]");
                    insertCommand.Parameters.AddWithValue("@Members", JsonSerializer.Serialize(new List<int?> { userId }));
                    await insertCommand.ExecuteNonQueryAsync();
                }
            }

            await transaction.CommitAsync();
        }
    }
}