using Halal_Station_Remastered.Utils.Authorization;
using MySql.Data.MySqlClient;
using System.Data;

namespace Halal_Station_Remastered.Utils.Services.UserServices
{
    public class ChangeNicknameService
    {
        private static string _connectionString;

        public ChangeNicknameService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<bool> ChangeNicknameAsync(int? userId, string newNickname)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var fetchStateQuery = @"
                SELECT StateName, StateType, Value, OwnType
                FROM userstates
                WHERE UserId = @UserId AND StateName = 'account_rename_token'";

            (string stateName, int stateType, int currentValue, int ownType) = (null, 0, 0, 0);

            using (var fetchStateCommand = new MySqlCommand(fetchStateQuery, connection))
            {
                fetchStateCommand.Parameters.AddWithValue("@UserId", userId.Value);

                using (var reader = await fetchStateCommand.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        stateName = reader.GetString("StateName");
                        stateType = reader.GetInt32("StateType");
                        currentValue = reader.GetInt32("Value");
                        ownType = reader.GetInt32("OwnType");
                    }
                }
            }

            var initialValue = currentValue;
            var resultingValue = initialValue - 1;
            var deltaValue = 1;

            var updateUserQuery = "UPDATE users SET Nickname = @Nickname WHERE UserId = @UserId";
            using (var updateUserCommand = new MySqlCommand(updateUserQuery, connection))
            {
                updateUserCommand.Parameters.AddWithValue("@Nickname", newNickname);
                updateUserCommand.Parameters.AddWithValue("@UserId", userId.Value);
                await updateUserCommand.ExecuteNonQueryAsync();
            }

            var updateStateQuery = @"
                UPDATE userstates
                SET Value = @ResultingValue
                WHERE UserId = @UserId AND StateName = 'account_rename_token'";

            using (var updateStateCommand = new MySqlCommand(updateStateQuery, connection))
            {
                updateStateCommand.Parameters.AddWithValue("@ResultingValue", resultingValue);
                updateStateCommand.Parameters.AddWithValue("@UserId", userId.Value);
                await updateStateCommand.ExecuteNonQueryAsync();
            }

            var sessionId = Guid.NewGuid().ToString();
            var referenceId = Guid.NewGuid().ToString();
            var timestamp = UnixTime.GetNow();

            var upsertTransactionQuery = @"
                INSERT INTO transactions (UserId, OfferId, InitialValue, ResultingValue, DeltaValue, OperationType, SessionId, ReferenceId, TimeStamp, DescId, StateName, StateType, OwnType)
                VALUES (@UserId, @OfferId, @InitialValue, @ResultingValue, @DeltaValue, 0, @SessionId, @ReferenceId, @TimeStamp, 0, @StateName, @StateType, @OwnType)
                ON DUPLICATE KEY UPDATE
                    InitialValue = @InitialValue,
                    ResultingValue = @ResultingValue,
                    DeltaValue = @DeltaValue,
                    TimeStamp = @TimeStamp;";

            using (var upsertTransactionCommand = new MySqlCommand(upsertTransactionQuery, connection))
            {
                upsertTransactionCommand.Parameters.AddWithValue("@UserId", userId.Value);
                upsertTransactionCommand.Parameters.AddWithValue("@OfferId", stateName);
                upsertTransactionCommand.Parameters.AddWithValue("@InitialValue", initialValue);
                upsertTransactionCommand.Parameters.AddWithValue("@ResultingValue", resultingValue);
                upsertTransactionCommand.Parameters.AddWithValue("@DeltaValue", deltaValue);
                upsertTransactionCommand.Parameters.AddWithValue("@OperationType", 0);
                upsertTransactionCommand.Parameters.AddWithValue("@SessionId", sessionId);
                upsertTransactionCommand.Parameters.AddWithValue("@ReferenceId", referenceId);
                upsertTransactionCommand.Parameters.AddWithValue("@TimeStamp", timestamp);
                upsertTransactionCommand.Parameters.AddWithValue("@StateName", stateName);
                upsertTransactionCommand.Parameters.AddWithValue("@StateType", stateType);
                upsertTransactionCommand.Parameters.AddWithValue("@OwnType", ownType);
                await upsertTransactionCommand.ExecuteNonQueryAsync();
            }

            return true;
        }
    }
}