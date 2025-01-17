using MySql.Data.MySqlClient;
using System.Data;

namespace Halal_Station_Remastered.Utils.Services.UserServices
{
    public class PrimaryStateService
    {
        private static string _connectionString;

        public PrimaryStateService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<List<object>> GetUsersPrimaryStatesAsync(List<UserId> users)
        {
            var result = new List<object>();

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            foreach (var user in users)
            {
                var userId = user.Id;
                var query = "SELECT * FROM primarystates WHERE UserId = @UserId";

                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (reader.Read())
                        {
                            result.Add(new
                            {
                                User = new { Id = reader.GetInt32("UserId") },
                                Xp = reader.IsDBNull(reader.GetOrdinal("Xp")) ? 0 : reader.GetInt32("Xp"),
                                Kills = reader.IsDBNull(reader.GetOrdinal("Kills")) ? 0 : reader.GetInt32("Kills"),
                                Deaths = reader.IsDBNull(reader.GetOrdinal("Deaths")) ? 0 : reader.GetInt32("Deaths"),
                                Assists = reader.IsDBNull(reader.GetOrdinal("Assists")) ? 0 : reader.GetInt32("Assists"),
                                Suicides = reader.IsDBNull(reader.GetOrdinal("Suicides")) ? 0 : reader.GetInt32("Suicides"),
                                TotalMatches = reader.IsDBNull(reader.GetOrdinal("TotalMatches")) ? 0 : reader.GetInt32("TotalMatches"),
                                Victories = reader.IsDBNull(reader.GetOrdinal("Victories")) ? 0 : reader.GetInt32("Victories"),
                                Defeats = reader.IsDBNull(reader.GetOrdinal("Defeats")) ? 0 : reader.GetInt32("Defeats"),
                                TotalWP = reader.IsDBNull(reader.GetOrdinal("TotalWP")) ? 0 : reader.GetInt32("TotalWP"),
                                TotalTimePlayed = reader.IsDBNull(reader.GetOrdinal("TotalTimePlayed")) ? 0 : reader.GetInt32("TotalTimePlayed"),
                                TotalTimeOnline = reader.IsDBNull(reader.GetOrdinal("TotalTimeOnline")) ? 0 : reader.GetInt32("TotalTimeOnline")
                            });
                        }
                        else
                        {
                            reader.Close();

                            var insertQuery = "INSERT INTO primarystates (UserId, Xp, Kills, Deaths, Assists, Suicides, TotalMatches, Victories, Defeats, TotalWP, TotalTimePlayed, TotalTimeOnline) " +
                                              "VALUES (@UserId, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)";
                            using (var insertCmd = new MySqlCommand(insertQuery, connection))
                            {
                                insertCmd.Parameters.AddWithValue("@UserId", userId);
                                await insertCmd.ExecuteNonQueryAsync();
                            }

                            result.Add(new
                            {
                                User = new { Id = userId },
                                Xp = 0,
                                Kills = 0,
                                Deaths = 0,
                                Assists = 0,
                                Suicides = 0,
                                TotalMatches = 0,
                                Victories = 0,
                                Defeats = 0,
                                TotalWP = 0,
                                TotalTimePlayed = 0,
                                TotalTimeOnline = 0
                            });
                        }
                    }
                }
            }
            return result;
        }
    }
}