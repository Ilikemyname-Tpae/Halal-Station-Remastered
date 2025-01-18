using MySql.Data.MySqlClient;

namespace Halal_Station_Remastered.Utils.Services.PresenceServices
{
    public class PartySetGameDataService
    {
        private static string _connectionString;

        public PartySetGameDataService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<bool> UpdateGameDataAsync(int? userId, string newGameData)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            const string checkPartySql = @"
            SELECT p.Id
            FROM party p
            WHERE p.UserId = @UserId";

            using var checkPartyCommand = new MySqlCommand(checkPartySql, connection);
            checkPartyCommand.Parameters.AddWithValue("@UserId", userId);

            var partyId = (await checkPartyCommand.ExecuteScalarAsync())?.ToString();

            if (partyId == null)
            {
                return false;
            }

            const string updateSql = @"
            UPDATE party
            SET GameData = @GameData
            WHERE Id = @PartyId";

            using var updateCommand = new MySqlCommand(updateSql, connection);
            updateCommand.Parameters.AddWithValue("@GameData", newGameData);
            updateCommand.Parameters.AddWithValue("@PartyId", partyId);

            var rowsAffected = await updateCommand.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }
    }
}