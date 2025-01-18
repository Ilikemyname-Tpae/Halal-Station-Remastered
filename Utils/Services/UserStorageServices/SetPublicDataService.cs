using MySql.Data.MySqlClient;

namespace Halal_Station_Remastered.Utils.Services.UserStorageServices
{
    public class SetPublicDataService
    {
        private readonly string _connectionString;

        public SetPublicDataService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task UpdateUserLoadoutAsync(int? userId, string armorLoadout, string weaponLoadout, string customization)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
                INSERT INTO userloadouts (UserId, ArmorLoadout, Customization, WeaponLoadout)
                VALUES (@UserId, @ArmorLoadout, @Customization, @WeaponLoadout)
                ON DUPLICATE KEY UPDATE
                    ArmorLoadout = COALESCE(@ArmorLoadout, ArmorLoadout),
                    Customization = COALESCE(@Customization, Customization),
                    WeaponLoadout = COALESCE(@WeaponLoadout, WeaponLoadout);
            ";

            using (var command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@ArmorLoadout", (object)armorLoadout ?? DBNull.Value);
                command.Parameters.AddWithValue("@Customization", (object)customization ?? DBNull.Value);
                command.Parameters.AddWithValue("@WeaponLoadout", (object)weaponLoadout ?? DBNull.Value);

                await command.ExecuteNonQueryAsync();
            }
        }
    }
}