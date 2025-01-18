using MySql.Data.MySqlClient;

namespace Halal_Station_Remastered.Utils.Services.PresenceServices
{
    public class PartyLeaveService
    {
        private static string _connectionString;
        public PartyLeaveService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<(string partyId, bool isOwner, int matchmakeState, string gameData, bool success)> LeavePartyAsync(int? userId)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var selectQuery = @"
                SELECT p.PartyId, p.Owner, p.MatchmakeState, p.GameData
                FROM party p
                WHERE p.UserId = @UserId";

            using (var selectCommand = new MySqlCommand(selectQuery, connection))
            {
                selectCommand.Parameters.AddWithValue("@UserId", userId);

                using (var reader = await selectCommand.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        var partyId = reader.GetString(reader.GetOrdinal("PartyId"));
                        var isOwner = reader.GetBoolean(reader.GetOrdinal("Owner"));
                        var matchmakeState = reader.GetInt32(reader.GetOrdinal("MatchmakeState"));
                        var gameData = reader.GetString(reader.GetOrdinal("GameData"));

                        reader.Close();

                        var updateQuery = @"
                            UPDATE party
                            SET MatchmakeState = 0
                            WHERE PartyId = @PartyId AND UserId = @UserId";

                        using (var updateCommand = new MySqlCommand(updateQuery, connection))
                        {
                            updateCommand.Parameters.AddWithValue("@PartyId", partyId);
                            updateCommand.Parameters.AddWithValue("@UserId", userId);

                            var rowsAffected = await updateCommand.ExecuteNonQueryAsync();
                            if (rowsAffected > 0)
                            {
                                return (partyId, isOwner, matchmakeState, gameData, true);
                            }
                        }
                    }
                }
            }
            return (null, false, 0, null, false);
        }
    }
}