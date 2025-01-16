using MySql.Data.MySqlClient;
using System.Data;
using System.Text.Json;

namespace Halal_Station_Remastered.Utils.Services.UserStorageServices
{
    public class UserPrivateDataService
    {
        private readonly string _connectionString;

        public UserPrivateDataService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<(object layout, object version, object[] data)> GetUserPrivateDataAsync(int? userId)
        {
            object layout = 1;
            object version = 0;
            object[] data = new object[] { };

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new MySqlCommand("SELECT Preferences FROM userprivate WHERE UserId = @UserId", connection);

            command.Parameters.AddWithValue("@UserId", userId ?? 0);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var preferencesJson = reader.GetString("Preferences");
                var preferencesDoc = JsonDocument.Parse(preferencesJson);

                if (preferencesDoc.RootElement.TryGetProperty("Layout", out JsonElement layoutElement))
                {
                    layout = layoutElement.GetInt32();
                }
                if (preferencesDoc.RootElement.TryGetProperty("Version", out JsonElement versionElement))
                {
                    version = versionElement.GetInt32();
                }
                if (preferencesDoc.RootElement.TryGetProperty("Data", out JsonElement dataElement))
                {
                    data = JsonSerializer.Deserialize<object[]>(dataElement.GetRawText());
                }
            }
            return (layout, version, data);
        }
    }
}