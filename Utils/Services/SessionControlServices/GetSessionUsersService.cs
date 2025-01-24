using MySql.Data.MySqlClient;

namespace Halal_Station_Remastered.Utils.Services.SessionControlServices
{
    public class GetSessionUsersService
    {
        private readonly string _connectionString;

        public GetSessionUsersService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<List<int>> GetUserIdsWithMatchmakeStateAsync()
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            const string query = @"
                SELECT UserId
                FROM Party
                WHERE MatchmakeState = 2
                ORDER BY UserId ASC";

            using var command = new MySqlCommand(query, connection);

            var userIds = new List<int>();

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                userIds.Add(reader.GetInt32(reader.GetOrdinal("UserId")));
            }

            if (!userIds.Contains(1))
            {
                userIds.Insert(0, 1);
            }

            return userIds;
        }
    }
}