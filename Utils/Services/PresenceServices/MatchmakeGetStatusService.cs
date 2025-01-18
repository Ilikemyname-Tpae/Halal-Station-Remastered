using MySql.Data.MySqlClient;

namespace Halal_Station_Remastered.Utils.Services.PresenceServices
{
    public class MatchmakeGetStatusService
    {
        private readonly string _connectionString;

        public MatchmakeGetStatusService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<List<object>> GetAllMembersWithMatchmakeStateAsync()
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            const string query = @"
                SELECT UserId, PartyId, Owner, MatchmakeState
                FROM Party
                WHERE MatchmakeState = 2";

            using var command = new MySqlCommand(query, connection);

            var members = new List<object>();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                members.Add(new
                {
                    User = new
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("UserId"))
                    },
                    Party = new
                    {
                        Id = reader.GetString(reader.GetOrdinal("PartyId"))
                    },
                    IsOwner = reader.GetBoolean(reader.GetOrdinal("Owner")),
                    MatchmakeState = reader.GetInt32(reader.GetOrdinal("MatchmakeState"))
                });
            }
            return members;
        }
    }
}