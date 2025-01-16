using MySql.Data.MySqlClient;

namespace Halal_Station_Remastered.Utils.Services.GameStatisticServices
{
    public class GetUserChallengesService
    {
        private readonly string _connectionString;

        public GetUserChallengesService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<List<Challenge>> GetChallengesAsync(int? userId)
        {
            var challenges = new List<Challenge>();

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            const string query = @"
                SELECT `ChallengeId`, `Version`, `Progress`, `CounterName`, `CurrValue`, `MaxValue`, 
                       `FinishedAtUnixMilliseconds`, `StartDateUnixMilliseconds`, `EndDateUnixMilliseconds`, 
                       `RewardName`, `RewardCount`
                FROM `userchallenges`
                WHERE `UserId` = @UserId";

            using (var command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var challenge = new Challenge
                        {
                            ChallengeId = reader["ChallengeId"]?.ToString(),
                            Progress = reader["Progress"] != DBNull.Value ? Convert.ToInt32(reader["Progress"]) : 0,
                            FinishedAtUnixMilliseconds = reader["FinishedAtUnixMilliseconds"] != DBNull.Value
                                ? Convert.ToInt64(reader["FinishedAtUnixMilliseconds"]) : 0,
                            StartDateUnixMilliseconds = reader["StartDateUnixMilliseconds"] != DBNull.Value
                                ? Convert.ToInt64(reader["StartDateUnixMilliseconds"]) : 0,
                            EndDateUnixMilliseconds = reader["EndDateUnixMilliseconds"] != DBNull.Value
                                ? Convert.ToInt64(reader["EndDateUnixMilliseconds"]) : 0,
                            Counters = new List<Counter>
                            {
                                new Counter
                                {
                                    CounterName = reader["CounterName"]?.ToString(),
                                    CurrValue = reader["CurrValue"] != DBNull.Value ? Convert.ToInt32(reader["CurrValue"]) : 0,
                                    MaxValue = reader["MaxValue"] != DBNull.Value ? Convert.ToInt32(reader["MaxValue"]) : 0
                                }
                            },
                            Rewards = new List<Reward>
                            {
                                new Reward
                                {
                                    Name = reader["RewardName"]?.ToString(),
                                    Count = reader["RewardCount"] != DBNull.Value ? Convert.ToInt32(reader["RewardCount"]) : 0
                                }
                            }
                        };
                        challenges.Add(challenge);
                    }
                }
            }
            return challenges;
        }
    }
}
