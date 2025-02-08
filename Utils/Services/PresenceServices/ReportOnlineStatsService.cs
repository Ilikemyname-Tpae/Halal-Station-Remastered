using MySql.Data.MySqlClient;

namespace Halal_Station_Remastered.Utils.Services.PresenceServices
{
    public class ReportOnlineStatsService
    {
        private static string _connectionString;

        public ReportOnlineStatsService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<(int usersFound, int usersIngame)> GetOnlineStatsAsync()
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            int usersFound = await GetMainMenuUsersAsync(connection);
            int usersIngame = await GetInGameUsersAsync(connection);

            return (usersFound, usersIngame);
        }

        private async Task<int> GetMainMenuUsersAsync(MySqlConnection connection)
        {
            const string mainMenuQuery = @"
                SELECT COUNT(DISTINCT u.UserId) as MainMenuCount
                FROM users u
                LEFT JOIN Party p ON u.UserId = p.UserId
                WHERE u.State > 0 
                AND (p.MatchmakeState = 0 OR p.MatchmakeState IS NULL)";

            using var command = new MySqlCommand(mainMenuQuery, connection);
            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return reader.GetInt32(reader.GetOrdinal("MainMenuCount"));
            }
            return 0;
        }

        private async Task<int> GetInGameUsersAsync(MySqlConnection connection)
        {
            const string inGameQuery = @"
                SELECT COUNT(DISTINCT u.UserId) as InGameCount
                FROM users u
                INNER JOIN Party p ON u.UserId = p.UserId
                WHERE u.State > 0 
                AND p.MatchmakeState > 0";

            using var command = new MySqlCommand(inGameQuery, connection);
            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return reader.GetInt32(reader.GetOrdinal("InGameCount"));
            }
            return 0;
        }
    }
}