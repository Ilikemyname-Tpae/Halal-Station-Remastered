using MySql.Data.MySqlClient;
using System.Data;
using System.Text.Json;

namespace Halal_Station_Remastered.Utils.Services.UserStorageServices
{
    public class GetPublicDataService
    {
        private readonly string _connectionString;

        public GetPublicDataService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<List<object>> GetPublicDataAsync(UserRequest requestData)
        {
            var responseDataList = new List<object>();

            foreach (var user in requestData.Users)
            {
                var userLoadouts = await GetUserLoadoutsFromDatabaseAsync(user.Id);

                if (userLoadouts == null)
                {
                    throw new KeyNotFoundException($"User loadouts not found for User ID: {user.Id}");
                }

                string responseData;
                switch (requestData.ContainerName.ToLower())
                {
                    case "customizations":
                        responseData = userLoadouts.Customization;
                        break;
                    case "weapon_loadouts":
                        responseData = userLoadouts.WeaponLoadout;
                        break;
                    case "armor_loadouts":
                        responseData = userLoadouts.ArmorLoadout;
                        break;
                    default:
                        throw new ArgumentException("Invalid container name.");
                }

                var perUserDataObject = JsonSerializer.Deserialize<JsonElement>(responseData);
                responseDataList.Add(new
                {
                    User = new { Id = user.Id },
                    PerUserData = perUserDataObject
                });
            }

            return responseDataList;
        }

        private async Task<UserLoadouts> GetUserLoadoutsFromDatabaseAsync(int userId)
        {
            const string query = @"
            SELECT UserId, ArmorLoadout, Customization, WeaponLoadout, Preferences
            FROM userloadouts
            WHERE UserId = @UserId";

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new UserLoadouts
                            {
                                UserId = reader.GetInt32("UserId"),
                                ArmorLoadout = reader.GetString("ArmorLoadout"),
                                Customization = reader.GetString("Customization"),
                                WeaponLoadout = reader.GetString("WeaponLoadout"),
                                Preferences = reader.GetString("Preferences")
                            };
                        }
                    }
                }
            }
            return null;
        }
    }
}