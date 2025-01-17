using MySql.Data.MySqlClient;

namespace Halal_Station_Remastered.Utils.Services.UserServices
{
    public class GetUsersByNicknameService
    {
        private static string _connectionString;

        public GetUsersByNicknameService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<List<object>> GetUsersByNicknameAsync(string nicknamePrefix, int maxResults)
        {
            var users = new List<object>();

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            string query = "SELECT UserId FROM users WHERE Nickname LIKE @nicknamePrefix LIMIT @maxResults";
            using (var cmd = new MySqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@nicknamePrefix", $"{nicknamePrefix}%");
                cmd.Parameters.AddWithValue("@maxResults", maxResults);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        users.Add(new
                        {
                            Id = reader["UserId"]
                        });
                    }
                }
            }
            return users;
        }
    }
}