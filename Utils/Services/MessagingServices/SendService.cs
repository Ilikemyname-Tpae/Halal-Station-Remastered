using MySql.Data.MySqlClient;
using System.Text.Json;

namespace Halal_Station_Remastered.Utils.Services.MessagingServices
{
    public class SendService
    {
        private readonly string _connectionString;

        public SendService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<bool> AddMessageToChannelAsync(int? userId, string channelName, string message)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                var selectQuery = "SELECT Id, Messages FROM Channels WHERE Name = @Name";
                using var selectCommand = new MySqlCommand(selectQuery, connection, (MySqlTransaction)transaction);
                selectCommand.Parameters.AddWithValue("@Name", channelName);

                int channelId;
                List<dynamic> messageList;

                using (var reader = await selectCommand.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        channelId = reader.GetInt32(reader.GetOrdinal("Id"));
                        var messages = reader.GetString(reader.GetOrdinal("Messages"));

                        messageList = string.IsNullOrEmpty(messages)
                            ? new List<dynamic>()
                            : JsonSerializer.Deserialize<List<dynamic>>(messages);
                    }
                    else
                    {
                        return false;
                    }
                }

                var newMessage = new
                {
                    From = new { Id = userId },
                    Text = message,
                    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                };

                messageList.Add(newMessage);

                var updateQuery = "UPDATE Channels SET Messages = @Messages WHERE Id = @Id";
                using var updateCommand = new MySqlCommand(updateQuery, connection, (MySqlTransaction)transaction);
                updateCommand.Parameters.AddWithValue("@Messages", JsonSerializer.Serialize(messageList));
                updateCommand.Parameters.AddWithValue("@Id", channelId);

                await updateCommand.ExecuteNonQueryAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}