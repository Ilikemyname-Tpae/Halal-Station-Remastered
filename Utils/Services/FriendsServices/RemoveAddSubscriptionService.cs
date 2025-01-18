using MySql.Data.MySqlClient;

namespace Halal_Station_Remastered.Utils.Services.FriendsServices
{
    public class RemoveAddSubscriptionService
    {
        private readonly string _connectionString;

        public RemoveAddSubscriptionService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<List<object>> RemoveSubscriptionAsync(int? userId, int userIdToRemove)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            const string deleteQuery = @"
                DELETE FROM friends
                WHERE User = @UserId AND Friend = @UserIdToRemove;";
            using (var deleteCommand = new MySqlCommand(deleteQuery, connection))
            {
                deleteCommand.Parameters.AddWithValue("@UserId", userId);
                deleteCommand.Parameters.AddWithValue("@UserIdToRemove", userIdToRemove);
                await deleteCommand.ExecuteNonQueryAsync();
            }

            return await GetSubscriptionsAsync(userId, connection);
        }

        public async Task<List<object>> AddSubscriptionAsync(int? userId, int userIdToAdd)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            const string insertQuery = @"
                INSERT INTO friends (User, Friend)
                VALUES (@UserId, @UserIdToAdd);";
            using (var insertCommand = new MySqlCommand(insertQuery, connection))
            {
                insertCommand.Parameters.AddWithValue("@UserId", userId);
                insertCommand.Parameters.AddWithValue("@UserIdToAdd", userIdToAdd);
                await insertCommand.ExecuteNonQueryAsync();
            }

            return await GetSubscriptionsAsync(userId, connection);
        }

        private async Task<List<object>> GetSubscriptionsAsync(int? userId, MySqlConnection connection)
        {
            const string query = @"
                SELECT Friend
                FROM friends
                WHERE User = @UserId";

            var subscriptions = new List<object>();

            using (var selectCommand = new MySqlCommand(query, connection))
            {
                selectCommand.Parameters.AddWithValue("@UserId", userId);
                using (var reader = await selectCommand.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        subscriptions.Add(new
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Friend"))
                        });
                    }
                }
            }
            return subscriptions;
        }
    }
}