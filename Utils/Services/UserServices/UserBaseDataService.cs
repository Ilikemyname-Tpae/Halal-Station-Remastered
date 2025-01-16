using MySql.Data.MySqlClient;

namespace Halal_Station_Remastered.Utils.Services.UserServices
{
    public class UserBaseDataService
    {
        private readonly string _connectionString;

        public UserBaseDataService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<List<object>> GetUsersBaseDataAsync(List<int> userIds)
        {
            var usersData = new List<object>();

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            var parameterizedIds = string.Join(", ", userIds.Select((id, index) => $"@UserId{index}"));
            string query = $"SELECT UserId, Nickname, Level, BattleTag, ClanTag FROM Users WHERE UserId IN ({parameterizedIds})";

            using (var command = new MySqlCommand(query, connection))
            {
                for (int i = 0; i < userIds.Count; i++)
                {
                    command.Parameters.AddWithValue($"@UserId{i}", userIds[i]);
                }

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        int userId = reader.GetInt32(reader.GetOrdinal("UserId"));
                        var userData = new
                        {
                            User = new { Id = userId },
                            Nickname = reader.GetString(reader.GetOrdinal("Nickname")),
                            Level = reader.GetInt32(reader.GetOrdinal("Level")),
                            BattleTag = reader.GetString(reader.GetOrdinal("BattleTag")),
                            Clan = new { Id = 0 },
                            ClanTag = reader.GetString(reader.GetOrdinal("ClanTag"))
                        };
                        usersData.Add(userData);
                    }
                }
            }
            return usersData;
        }
    }
}