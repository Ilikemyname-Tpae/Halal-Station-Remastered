using MySql.Data.MySqlClient;
using System.Data;

namespace Halal_Station_Remastered.Utils.Services.PresenceServices
{
    public class PresenceConnectService
    {
        private static string _connectionString;

        public PresenceConnectService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<(string partyId, int matchmakeState, byte[] gameData, List<object> sessionMembers)> GetOrCreatePartyAsync(int? userId)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new MySqlCommand("SELECT PartyId, Owner, MatchmakeState, GameData FROM party WHERE UserId = @UserId", connection);
            command.Parameters.AddWithValue("@UserId", userId);

            using var reader = await command.ExecuteReaderAsync();
            if (!reader.Read())
            {
                reader.Close();
                var newPartyId = Guid.NewGuid().ToString();
                var newMatchmakeState = 0;
                var newGameData = new byte[100];

                var insertCommand = new MySqlCommand("INSERT INTO party (PartyId, UserId, Owner, MatchmakeState, GameData) VALUES (@PartyId, @UserId, @Owner, @MatchmakeState, @GameData)", connection);
                insertCommand.Parameters.AddWithValue("@PartyId", newPartyId);
                insertCommand.Parameters.AddWithValue("@UserId", userId);
                insertCommand.Parameters.AddWithValue("@Owner", true);
                insertCommand.Parameters.AddWithValue("@MatchmakeState", newMatchmakeState);
                insertCommand.Parameters.AddWithValue("@GameData", Convert.ToBase64String(newGameData));
                await insertCommand.ExecuteNonQueryAsync();

                return await GetSessionMembersAsync(newPartyId, newMatchmakeState, newGameData, connection);
            }
            else
            {
                var partyId = reader.GetString("PartyId");
                var matchmakeState = reader.GetInt32("MatchmakeState");
                var gameDataBase64 = reader.GetString("GameData");
                var gameData = Convert.FromBase64String(gameDataBase64);

                reader.Close();

                return await GetSessionMembersAsync(partyId, matchmakeState, gameData, connection);
            }
        }

        private async Task<(string partyId, int matchmakeState, byte[] gameData, List<object> sessionMembers)> GetSessionMembersAsync(string partyId, int matchmakeState, byte[] gameData, MySqlConnection connection)
        {
            var sessionMembersCommand = new MySqlCommand("SELECT UserId, Owner FROM party WHERE PartyId = @PartyId", connection);
            sessionMembersCommand.Parameters.AddWithValue("@PartyId", partyId);

            var sessionMembersReader = await sessionMembersCommand.ExecuteReaderAsync();
            var sessionMembers = new List<object>();
            while (sessionMembersReader.Read())
            {
                sessionMembers.Add(new
                {
                    User = new
                    {
                        Id = sessionMembersReader.GetInt32("UserId")
                    },
                    IsOwner = sessionMembersReader.GetBoolean("Owner")
                });
            }
            sessionMembersReader.Close();

            return (partyId, matchmakeState, gameData, sessionMembers);
        }
    }
}