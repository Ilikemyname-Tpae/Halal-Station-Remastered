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

        public async Task<(string partyId, bool isOwner, int matchmakeState, string gameData)> GetPartyStatusAsync(int? userId)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var sql = @"
                SELECT p.PartyId, p.Owner, p.MatchmakeState, p.GameData
                FROM party p
                WHERE p.UserId = @UserId";

            var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@UserId", userId);

            using var reader = await command.ExecuteReaderAsync();
            if (reader.Read())
            {
                var partyId = reader.GetString("PartyId");
                var isOwner = reader.GetBoolean("Owner");
                var matchmakeState = reader.GetInt32("MatchmakeState");
                var gameData = reader.GetString("GameData");

                return (partyId, isOwner, matchmakeState, gameData);
            }
            return (null, false, 0, null);
        }
    }
}