using MySql.Data.MySqlClient;
using System.Data;

namespace Halal_Station_Remastered.Utils.Services.PresenceServices
{
    public class PartyService
    {
        private static string _connectionString;

        public PartyService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<(string partyId, List<(int userId, bool isOwner)> members, int matchmakeState, string gameData)> GetPartyStatusAsync(int? userId)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var getPartyIdSql = @"
            SELECT PartyId 
            FROM party 
            WHERE UserId = @UserId";

            var partyIdCommand = new MySqlCommand(getPartyIdSql, connection);
            partyIdCommand.Parameters.AddWithValue("@UserId", userId);
            var partyId = (string)await partyIdCommand.ExecuteScalarAsync();

            if (partyId == null)
            {
                return (null, new List<(int, bool)>(), 0, null);
            }

            var sql = @"
            SELECT p.UserId, p.Owner, p.MatchmakeState, p.GameData
            FROM party p
            WHERE p.PartyId = @PartyId";

            var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@PartyId", partyId);

            var members = new List<(int userId, bool isOwner)>();
            int matchmakeState = 0;
            string gameData = null;

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var memberId = reader.GetInt32("UserId");
                var isOwner = reader.GetBoolean("Owner");
                members.Add((memberId, isOwner));

                matchmakeState = reader.GetInt32("MatchmakeState");
                gameData = reader.GetString("GameData");
            }

            return (partyId, members, matchmakeState, gameData);
        }
    }
}