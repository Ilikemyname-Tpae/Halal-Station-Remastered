using MySql.Data.MySqlClient;
using System.Data;

namespace Halal_Station_Remastered.Utils.Services.AuthorizationServices
{
    public class AuthorizationUser
    {
        private static string _connectionString;
        public AuthorizationUser(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task CreateUser(string username, string password)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                var query = "INSERT INTO Users (Name, Password, Nickname, BattleTag, ClanTag, Level) VALUES (@Name, @Password, @Nickname, @BattleTag, @ClanTag, @Level); SELECT LAST_INSERT_ID();";
                using var command = new MySqlCommand(query, connection, transaction);
                command.Parameters.AddWithValue("@Name", username);
                command.Parameters.AddWithValue("@Password", hashedPassword);
                command.Parameters.AddWithValue("@Nickname", "Welcome!");
                command.Parameters.AddWithValue("@BattleTag", "TEST");
                command.Parameters.AddWithValue("@Level", 1);
                command.Parameters.AddWithValue("@ClanTag", "TEST");

                var userId = Convert.ToInt32(await command.ExecuteScalarAsync());

                var loadoutsQuery = "INSERT INTO userloadouts (ArmorLoadout, Customization, WeaponLoadout, Preferences, UserId) " +
                                    "SELECT ArmorLoadout, Customization, WeaponLoadout, Preferences, @UserId FROM newplayerdata LIMIT 1;";
                using var loadoutsCommand = new MySqlCommand(loadoutsQuery, connection, transaction);
                loadoutsCommand.Parameters.AddWithValue("@UserId", userId);

                await loadoutsCommand.ExecuteNonQueryAsync();

                var statesQuery = "INSERT INTO userstates (StateName, OwnType, Value, StateType, UserId) VALUES " +
                                  "('Credits', 0, 1000, 2, @UserId)," + "('Gold', 0, 100, 3, @UserId)," + "('class_select_token', 1, 1, 12, @UserId)," +
                                  "('weapon_loadout_0', 1, 1, 0, @UserId)," + "('armor_loadout_0', 1, 1, 0, @UserId)," + "('magnum', 1, 1, 4, @UserId)," +
                                  "('frag_grenade', 1, 1, 4, @UserId)," + "('Level', 0, 1, 9, @UserId)," + "('Level_Progress', 0, 0, 1, @UserId);" + "('account_rename_token', 1, 3, 12, @UserId);";
                using var statesCommand = new MySqlCommand(statesQuery, connection, transaction);
                statesCommand.Parameters.AddWithValue("@UserId", userId);

                await statesCommand.ExecuteNonQueryAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<(int UserId, string Password)> GetUser(string username)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = "SELECT UserId, Password FROM Users WHERE Name = @Name";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Name", username);

            using var reader = await command.ExecuteReaderAsync();
            return reader.Read() ? (reader.GetInt32("UserId"), reader.GetString("Password")) : (0, null);
        }

        public async Task<bool> UserExists(string username)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = "SELECT COUNT(*) FROM Users WHERE Name = @Name";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Name", username);

            var existingUserCount = (long)await command.ExecuteScalarAsync();
            return existingUserCount > 0;
        }
    }
}