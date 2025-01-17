using MySql.Data.MySqlClient;

namespace Halal_Station_Remastered.Utils.Services.UserStorageServices
{
    public class UserSetPrivateDataService
    {
        private static string _connectionString;

        public UserSetPrivateDataService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task SetPreferencesAsync(int? userId, string preferences)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new MySqlCommand("INSERT INTO userprivate (UserId, Preferences) VALUES (@UserId, @Preferences)", connection);
            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@Preferences", preferences);

            await command.ExecuteNonQueryAsync();
        }
    }
}