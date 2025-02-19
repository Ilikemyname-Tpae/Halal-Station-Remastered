using Halal_Station_Remastered.Utils.Requests.UserServices;
using MySql.Data.MySqlClient;
using System.Text.Json;

namespace Halal_Station_Remastered.Utils.Services.UserServices
{
    public class ApplyOfferListAndGetTransactionHistoryService
    {
        private readonly string _connectionString;

        public ApplyOfferListAndGetTransactionHistoryService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<(List<object> transactions, string errorMessage)> ProcessOffersAndGetTransactionsAsync(
            ApplyOfferListAndGetTransactionHistoryRequest offerRequest, int? userId)
        {
            if (!userId.HasValue)
                return (new List<object>(), "Invalid user ID");

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                var transactions = new List<object>();
                foreach (var offerId in offerRequest.OfferIds)
                {
                    var (offer, offerError) = await GetOfferDetailsAsync(connection, transaction, offerId);
                    if (offerError != null)
                        return (new List<object>(), offerError);

                    var currencyType = offerId.EndsWith("_cr") ? "Credits" : "Gold";
                    var initialValue = await GetUserCurrencyAsync(connection, transaction, userId.Value, currencyType);

                    if (initialValue < offer.Price)
                        return (new List<object>(), $"Insufficient {currencyType}");

                    var resultingValue = initialValue - offer.Price;
                    var transactionEntry = CreateTransactionEntry(offerId, initialValue, resultingValue, currencyType, offerRequest.HistoryFromTime);

                    await RecordTransactionAsync(connection, transaction, userId.Value, offerId, initialValue, resultingValue, offer.Price, transactionEntry);
                    await UpdateUserCurrencyAsync(connection, transaction, userId.Value, currencyType, resultingValue);

                    if (IsSpecialOffer(offerId))
                    {
                        await HandleSpecialOfferAsync(connection, transaction, userId.Value, offerId);
                    }

                    transactions.Add(transactionEntry);
                }

                await transaction.CommitAsync();
                return (transactions, null);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (new List<object>(), ex.Message);
            }
        }
        private async Task<(Offer offer, string error)> GetOfferDetailsAsync(MySqlConnection connection, MySqlTransaction transaction, string offerId)
        {
            var offersDirectoryPath = Path.Combine(AppContext.BaseDirectory, "JsonData", "Offers");
            var allItemOffers = new List<ItemOffer>();

            if (Directory.Exists(offersDirectoryPath))
            {
                var jsonFiles = Directory.GetFiles(offersDirectoryPath, "*.json", SearchOption.AllDirectories);
                foreach (var filePath in jsonFiles)
                {
                    var jsonContent = File.ReadAllText(filePath);
                    var itemOffer = JsonSerializer.Deserialize<ItemOffer>(jsonContent);
                    if (itemOffer != null)
                    {
                        allItemOffers.Add(itemOffer);
                    }
                }
            }

            var offer = allItemOffers
                .SelectMany(io => io.OfferLine.SelectMany(ol => ol.Offers))
                .FirstOrDefault(o => o.OfferId == offerId);

            if (offer == null)
            {
                return (null, $"Offer not found: {offerId}");
            }

            return (offer, null);
        }

        private async Task<int> GetUserCurrencyAsync(MySqlConnection connection, MySqlTransaction transaction, int userId, string currencyType)
        {
            const string query = @"SELECT Value FROM userstates WHERE UserId = @UserId AND StateName = @CurrencyType";
            using (var command = new MySqlCommand(query, connection, transaction))
            {
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@CurrencyType", currencyType);

                var result = await command.ExecuteScalarAsync();
                return result != null && int.TryParse(result.ToString(), out var value) ? value : 0;
            }
        }

        private async Task UpdateUserCurrencyAsync(MySqlConnection connection, MySqlTransaction transaction, int userId, string currencyType, int newValue)
        {
            const string query = @"UPDATE userstates SET Value = @NewValue WHERE UserId = @UserId AND StateName = @CurrencyType";

            using (var command = new MySqlCommand(query, connection, transaction))
            {
                command.Parameters.AddWithValue("@NewValue", newValue);
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@CurrencyType", currencyType);

                await command.ExecuteNonQueryAsync();
            }
        }
        private object CreateTransactionEntry(string offerId, int initialValue, int resultingValue, string currencyType, long timestamp)
        {
            return new
            {
                transactionItems = new[]
                {
                new
                {
                    stateName = offerId.Replace("_cr", ""),
                    stateType = currencyType == "Credits" ? 2 : 3,
                    ownType = offerId.StartsWith("challenge") ? 1 : 2,
                    operationType = 0,
                    initialValue = offerId.StartsWith("challenge") ? 0 : initialValue,
                    resultingValue = offerId.StartsWith("challenge") ? 0 : resultingValue,
                    deltaValue = offerId.StartsWith("challenge") ? 0 : initialValue - resultingValue,
                    descId = 0
                }
            },
                sessionId = Guid.NewGuid().ToString(),
                referenceId = Guid.NewGuid().ToString(),
                offerId = offerId,
                timeStamp = timestamp,
                operationType = 0,
                extendedInfoItems = new[] { new { Key = "", Value = "" } }
            };
        }

        private async Task RecordTransactionAsync(MySqlConnection connection, MySqlTransaction transaction,
            int userId, string offerId, int initialValue, int resultingValue, int price, dynamic transactionEntry)
        {
            const string query = @"
            INSERT INTO transactions 
            (UserId, OfferId, InitialValue, ResultingValue, DeltaValue, OperationType, 
             SessionId, ReferenceId, TimeStamp, StateName, StateType, OwnType, DescId)
            VALUES 
            (@UserId, @OfferId, @InitialValue, @ResultingValue, @DeltaValue, @OperationType,
             @SessionId, @ReferenceId, @TimeStamp, @StateName, @StateType, @OwnType, @DescId)";

            using var cmd = new MySqlCommand(query, connection, transaction);
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@OfferId", offerId);
            cmd.Parameters.AddWithValue("@InitialValue", initialValue);
            cmd.Parameters.AddWithValue("@ResultingValue", resultingValue);
            cmd.Parameters.AddWithValue("@DeltaValue", price);
            cmd.Parameters.AddWithValue("@OperationType", 0);
            cmd.Parameters.AddWithValue("@SessionId", transactionEntry.sessionId);
            cmd.Parameters.AddWithValue("@ReferenceId", transactionEntry.referenceId);
            cmd.Parameters.AddWithValue("@TimeStamp", transactionEntry.timeStamp);
            cmd.Parameters.AddWithValue("@StateName", offerId.Replace("_cr", ""));
            cmd.Parameters.AddWithValue("@StateType", 4);
            cmd.Parameters.AddWithValue("@DescId", 0);
            cmd.Parameters.AddWithValue("@OwnType", offerId.StartsWith("challenge") ? 1 : 2);

            await cmd.ExecuteNonQueryAsync();
        }

        private async Task HandleSpecialOfferAsync(MySqlConnection connection, MySqlTransaction transaction, int userId, string offerId)
        {
            if (IsKitOffer(offerId))
            {
                const string query = @"
                UPDATE userstates 
                SET Value = 0, OwnType = 0 
                WHERE UserId = @UserId AND StateName = 'class_select_token';";

                using var cmd = new MySqlCommand(query, connection, transaction);
                cmd.Parameters.AddWithValue("@UserId", userId);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        private bool IsSpecialOffer(string offerId)
        {
            return IsKitOffer(offerId) || offerId.StartsWith("challenge");
        }

        private bool IsKitOffer(string offerId)
        {
            return offerId == "ranger_kit_offer" ||
                   offerId == "sniper_kit_offer" ||
                   offerId == "tactician_kit_offer";
        }
    }
}