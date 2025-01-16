using MySql.Data.MySqlClient;

namespace Halal_Station_Remastered.Utils.Services.FriendsServices
{
    public class GetSubscriptionsService
    {
        private readonly string _connectionString;

        public GetSubscriptionsService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<List<object>> GetUserSubscriptionsAsync(int? userId)
        {
            var userList = new List<object>();

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            const string query = @"
    SELECT Friend
    FROM friends
    WHERE User = @UserId";

            using (var command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        int friendId = reader.IsDBNull(reader.GetOrdinal("Friend"))
                            ? 0
                            : reader.GetInt32(reader.GetOrdinal("Friend"));

                        userList.Add(new { Id = friendId });
                    }
                }
            }
            return userList;
        }
    }
}