using MySql.Data.MySqlClient;
using System.Data;

namespace Halal_Station_Remastered.Utils.Services.PresenceServices
{
    public class PartyJoinService
    {
        private static string _connectionString;

        public PartyJoinService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<(bool success, bool isOwner, int matchmakeState, string gameData)> JoinPartyAsync(int? userId, string partyId)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var deleteQuery = @"
        DELETE FROM party 
        WHERE UserId = @UserId";

            using var deleteCommand = new MySqlCommand(deleteQuery, connection);
            deleteCommand.Parameters.AddWithValue("@UserId", userId);
            await deleteCommand.ExecuteNonQueryAsync();

            var checkPartyQuery = @"
        SELECT MatchmakeState, GameData 
        FROM party 
        WHERE PartyId = @PartyId 
        LIMIT 1";

            using var checkCommand = new MySqlCommand(checkPartyQuery, connection);
            checkCommand.Parameters.AddWithValue("@PartyId", partyId);

            using var reader = await checkCommand.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                return (false, false, 0, null);
            }

            var matchmakeState = reader.GetInt32("MatchmakeState");
            var gameData = reader.GetString("GameData");
            reader.Close();

            var insertQuery = @"
        INSERT INTO party (PartyId, UserId, Owner, MatchmakeState, GameData)
        VALUES (@PartyId, @UserId, false, @MatchmakeState, @GameData)";

            using var insertCommand = new MySqlCommand(insertQuery, connection);
            insertCommand.Parameters.AddWithValue("@PartyId", partyId);
            insertCommand.Parameters.AddWithValue("@UserId", userId);
            insertCommand.Parameters.AddWithValue("@MatchmakeState", matchmakeState);
            insertCommand.Parameters.AddWithValue("@GameData", gameData);

            try
            {
                await insertCommand.ExecuteNonQueryAsync();
                return (true, false, matchmakeState, gameData);
            }
            catch (MySqlException)
            {
                return (false, false, 0, null);
            }
        }
    }
}