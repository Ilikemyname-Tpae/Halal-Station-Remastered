using MySql.Data.MySqlClient;
using System.Data;
using System.Text.Json;

namespace Halal_Station_Remastered.Utils.Services.MessagingServices
{
    public class LeaveChannelsService
    {
        private readonly string _connectionString;

        public LeaveChannelsService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<List<object>> RemoveUserFromChannelsAsync(int? userId, IEnumerable<string> channelNames)
        {
            var remainingChannels = new List<object>();

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            using var transaction = await connection.BeginTransactionAsync();

            foreach (var channelName in channelNames)
            {
                if (string.IsNullOrWhiteSpace(channelName)) continue;

                var selectQuery = "SELECT Id, Name, Members FROM Channels WHERE Name = @Name";
                using var selectCommand = new MySqlCommand(selectQuery, connection, transaction);
                selectCommand.Parameters.AddWithValue("@Name", channelName);

                using var reader = await selectCommand.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var channelId = reader.GetInt32("Id");
                    var existingMembers = reader.GetString("Members");

                    var membersList = JsonSerializer.Deserialize<List<int?>>(existingMembers) ?? new List<int?>();
                    if (membersList.Contains(userId))
                    {
                        membersList.Remove(userId);
                        reader.Close();

                        if (membersList.Count == 0)
                        {
                            var deleteQuery = "DELETE FROM Channels WHERE Id = @Id";
                            using var deleteCommand = new MySqlCommand(deleteQuery, connection, transaction);
                            deleteCommand.Parameters.AddWithValue("@Id", channelId);
                            await deleteCommand.ExecuteNonQueryAsync();
                        }
                        else
                        {
                            var updateQuery = "UPDATE Channels SET Members = @Members WHERE Id = @Id";
                            using var updateCommand = new MySqlCommand(updateQuery, connection, transaction);
                            updateCommand.Parameters.AddWithValue("@Members", JsonSerializer.Serialize(membersList));
                            updateCommand.Parameters.AddWithValue("@Id", channelId);
                            await updateCommand.ExecuteNonQueryAsync();
                        }
                    }
                    else
                    {
                        reader.Close();
                    }
                }
                else
                {
                    reader.Close();
                }
            }

            await transaction.CommitAsync();

            var remainingQuery = "SELECT Name, Version, Messages, Members FROM Channels WHERE JSON_CONTAINS(Members, CAST(@UserId AS JSON), '$')";
            using var remainingCommand = new MySqlCommand(remainingQuery, connection);
            remainingCommand.Parameters.AddWithValue("@UserId", userId);

            using var readerRemaining = await remainingCommand.ExecuteReaderAsync();
            while (await readerRemaining.ReadAsync())
            {
                var membersList = JsonSerializer.Deserialize<List<int?>>(readerRemaining.GetString("Members")) ?? new List<int?>();
                remainingChannels.Add(new
                {
                    Name = readerRemaining.GetString("Name"),
                    Version = readerRemaining.GetInt32("Version"),
                    Messages = JsonSerializer.Deserialize<List<object>>(readerRemaining.GetString("Messages")) ?? new List<object>(),
                    Members = membersList.Select(id => new { Id = id }).ToList()
                });
            }
            return remainingChannels;
        }
    }
}