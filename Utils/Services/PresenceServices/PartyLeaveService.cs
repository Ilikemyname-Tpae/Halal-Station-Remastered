using MySql.Data.MySqlClient;

namespace Halal_Station_Remastered.Utils.Services.PresenceServices
{
    public class PartyLeaveService
    {
        private static string _connectionString;
        public PartyLeaveService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<(string partyId, bool isOwner, int matchmakeState, string gameData, bool success)> LeavePartyAsync(int? userId)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            using var transaction = await connection.BeginTransactionAsync();
            try
            {
                var selectQuery = @"
                SELECT p.PartyId, p.Owner, p.MatchmakeState, p.GameData
                FROM party p
                WHERE p.UserId = @UserId";

                using (var selectCommand = new MySqlCommand(selectQuery, connection, transaction))
                {
                    selectCommand.Parameters.AddWithValue("@UserId", userId);

                    using (var reader = await selectCommand.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            var oldPartyId = reader.GetString(reader.GetOrdinal("PartyId"));
                            var isOwner = reader.GetBoolean(reader.GetOrdinal("Owner"));
                            var matchmakeState = reader.GetInt32(reader.GetOrdinal("MatchmakeState"));
                            var gameData = reader.GetString(reader.GetOrdinal("GameData"));

                            reader.Close();

                            var deleteQuery = @"
                            DELETE FROM party 
                            WHERE UserId = @UserId";

                            using (var deleteCommand = new MySqlCommand(deleteQuery, connection, transaction))
                            {
                                deleteCommand.Parameters.AddWithValue("@UserId", userId);
                                await deleteCommand.ExecuteNonQueryAsync();
                            }

                            var newPartyId = Guid.NewGuid().ToString();
                            var insertQuery = @"
                            INSERT INTO party (PartyId, UserId, Owner, MatchmakeState, GameData)
                            VALUES (@PartyId, @UserId, @Owner, @MatchmakeState, @GameData)";

                            using (var insertCommand = new MySqlCommand(insertQuery, connection, transaction))
                            {
                                insertCommand.Parameters.AddWithValue("@PartyId", newPartyId);
                                insertCommand.Parameters.AddWithValue("@UserId", userId);
                                insertCommand.Parameters.AddWithValue("@Owner", true);
                                insertCommand.Parameters.AddWithValue("@MatchmakeState", 0);
                                insertCommand.Parameters.AddWithValue("@GameData", gameData);

                                await insertCommand.ExecuteNonQueryAsync();
                            }

                            await transaction.CommitAsync();
                            return (newPartyId, true, 0, gameData, true);
                        }
                    }
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return (null, false, 0, null, false);
        }
    }
}