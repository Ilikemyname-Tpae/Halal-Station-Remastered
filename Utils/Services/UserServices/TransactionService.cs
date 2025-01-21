using MySql.Data.MySqlClient;

namespace Halal_Station_Remastered.Utils.Services.UserServices
{
    public class TransactionService
    {
        private static string _connectionString;

        public TransactionService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<List<object>> GetTransactionHistoryAsync(int? userId)
        {
            var transactionsList = new List<object>();

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var query = @"SELECT 
            UserId, OfferId, InitialValue, ResultingValue, DeltaValue, 
            OperationType, SessionId, ReferenceId, TimeStamp, 
            DescId, StateName, StateType, OwnType 
            FROM transactions
            WHERE UserId = @UserId";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId.Value);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            int resultingValue = Convert.ToInt32(reader["ResultingValue"]);
                            int updatedResultingValue = resultingValue - 5;
                            int ownType = updatedResultingValue <= 0 ? 0 : Convert.ToInt32(reader["OwnType"]);

                            var transactionItem = new
                            {
                                stateName = reader["StateName"].ToString(),
                                stateType = Convert.ToInt32(reader["StateType"]),
                                ownType = ownType,
                                operationType = Convert.ToInt32(reader["OperationType"]),
                                initialValue = Convert.ToInt32(reader["InitialValue"]),
                                resultingValue = Math.Max(updatedResultingValue, 0),
                                deltaValue = Convert.ToInt32(reader["DeltaValue"]),
                                descId = Convert.ToInt32(reader["DescId"])
                            };

                            var transaction = new
                            {
                                transactionItems = new[] { transactionItem },
                                sessionId = reader["SessionId"].ToString(),
                                referenceId = reader["ReferenceId"].ToString(),
                                offerId = reader["OfferId"].ToString(),
                                timeStamp = Convert.ToInt64(reader["TimeStamp"]),
                                operationType = Convert.ToInt32(reader["OperationType"]),
                                extendedInfoItems = new[] { new { Key = "", Value = "" } }
                            };

                            transactionsList.Add(transaction);

                            await UpdateResultingValueAsync(userId.Value, reader["OfferId"].ToString(), updatedResultingValue, ownType, resultingValue);
                        }
                    }
                }
            }
            return transactionsList;
        }

        private async Task UpdateResultingValueAsync(int userId, string offerId, int updatedResultingValue, int ownType, int currentResultingValue)
        {
            if (currentResultingValue <= 0)
            {
                return;
            }

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var updateCommand = new MySqlCommand(
                    @"UPDATE transactions 
              SET ResultingValue = @UpdatedResultingValue, OwnType = @OwnType 
              WHERE UserId = @UserId AND OfferId = @OfferId", connection);

                updateCommand.Parameters.AddWithValue("@UpdatedResultingValue", updatedResultingValue <= 0 ? 0 : updatedResultingValue);
                updateCommand.Parameters.AddWithValue("@OwnType", updatedResultingValue <= 0 ? 0 : ownType);
                updateCommand.Parameters.AddWithValue("@UserId", userId);
                updateCommand.Parameters.AddWithValue("@OfferId", offerId);

                await updateCommand.ExecuteNonQueryAsync();
            }
        }
    }
}