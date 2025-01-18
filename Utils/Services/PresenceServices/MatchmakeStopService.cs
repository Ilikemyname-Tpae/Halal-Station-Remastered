using MySql.Data.MySqlClient;

namespace Halal_Station_Remastered.Utils.Services.PresenceServices
{
    public class MatchmakeStopService
    {
        private static string _connectionString;

        public MatchmakeStopService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<bool> StopMatchmakingAsync(int? userId)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            const string findPartySql = @"
                SELECT PartyId
                FROM party
                WHERE UserId = @UserId";

            using (var findPartyCommand = new MySqlCommand(findPartySql, connection))
            {
                findPartyCommand.Parameters.AddWithValue("@UserId", userId);

                var partyId = await findPartyCommand.ExecuteScalarAsync() as string;

                if (string.IsNullOrEmpty(partyId))
                {
                    return false;
                }

                const string updateMatchmakeStateSql = @"
                    UPDATE party
                    SET MatchmakeState = 0
                    WHERE PartyId = @PartyId";

                using (var updateCommand = new MySqlCommand(updateMatchmakeStateSql, connection))
                {
                    updateCommand.Parameters.AddWithValue("@PartyId", partyId);
                    var rowsAffected = await updateCommand.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }
    }
}