using MySql.Data.MySqlClient;

namespace Halal_Station_Remastered.Utils.Services.UserServices
{
    public class UserService
    {
        private static string _connectionString;
        public UserService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<(List<UserState> userStates, string nickname)> GetUserStatesAsync(int? userId)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var userStates = new List<UserState>();
            string nickname = string.Empty;

            const string query = @"SELECT u.Nickname, us.StateName, us.OwnType, us.Value, us.StateType
                              FROM userstates us
                              JOIN Users u ON u.UserId = us.UserId
                              WHERE us.UserId = @UserId";

            using (var command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        if (string.IsNullOrEmpty(nickname))
                        {
                            nickname = reader.GetString(reader.GetOrdinal("Nickname"));
                        }

                        userStates.Add(new UserState
                        {
                            StateName = reader.GetString(reader.GetOrdinal("StateName")),
                            OwnType = reader.GetInt32(reader.GetOrdinal("OwnType")),
                            Value = reader.GetInt32(reader.GetOrdinal("Value")),
                            StateType = reader.GetInt32(reader.GetOrdinal("StateType"))
                        });
                    }
                }
            }
            return (userStates, nickname);
        }
    }
}