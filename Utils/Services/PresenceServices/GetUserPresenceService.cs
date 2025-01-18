using MySql.Data.MySqlClient;

namespace Halal_Station_Remastered.Utils.Services.PresenceServices
{
    public class GetUserPresenceService
    {
        private readonly string _connectionString;

        public GetUserPresenceService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<List<object>> GetUserPresencesAsync(IEnumerable<int> userIds)
        {
            var userPresences = new List<object>();

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                foreach (var userId in userIds)
                {
                    string query = "SELECT State, IsInvitable FROM users WHERE UserId = @userId";
                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var state = reader.GetInt32(reader.GetOrdinal("State"));
                                var isInvitable = reader.GetBoolean(reader.GetOrdinal("IsInvitable"));

                                userPresences.Add(new
                                {
                                    User = new
                                    {
                                        Id = userId
                                    },
                                    Data = new
                                    {
                                        State = state,
                                        IsInvitable = isInvitable
                                    }
                                });
                            }
                        }
                    }
                }
            }
            return userPresences;
        }
    }
}