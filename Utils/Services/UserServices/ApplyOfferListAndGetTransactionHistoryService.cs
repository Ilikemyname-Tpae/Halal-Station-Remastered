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

        public async Task<(List<object> transactions, string errorMessage)> ProcessOffersAndGetTransactionsAsync(ApplyOfferListAndGetTransactionHistoryRequest offerRequest, int? userId)
        {
            int initialValue;
            string currencyType = "Gold";
            int stateType = 3;

            var offersDirectoryPath = Path.Combine(AppContext.BaseDirectory, "JsonData", "Offers");
            var allItemOffers = LoadOffersFromJson(offersDirectoryPath);

            var matchingOffers = allItemOffers
                .SelectMany(io => io.OfferLine.SelectMany(ol => ol.Offers))
                .Where(o => offerRequest.OfferIds.Contains(o.OfferId))
                .ToList();

            var transactions = new List<object>();

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    try
                    {
                        currencyType = offerRequest.OfferIds.Any(id => id.EndsWith("_cr")) ? "Credits" : "Gold";
                        stateType = currencyType == "Credits" ? 2 : 3;

                        initialValue = await GetInitialCurrencyValueAsync(connection, transaction, userId, currencyType);

                        foreach (var offer in matchingOffers)
                        {
                            var resultingValue = initialValue - offer.Price;

                            var transactionEntry = new
                            {
                                transactionItems = new[]
                                {
                                    new
                                    {
                                        stateName = offer.OfferId.Replace("_cr", ""),
                                        stateType = stateType,
                                        ownType = 2,
                                        operationType = 0,
                                        initialValue = initialValue,
                                        resultingValue = resultingValue,
                                        deltaValue = offer.Price,
                                        descId = 0
                                    }
                                },
                                sessionId = Guid.NewGuid().ToString(),
                                referenceId = Guid.NewGuid().ToString(),
                                offerId = offer.OfferId,
                                timeStamp = offerRequest.HistoryFromTime,
                                operationType = 0,
                                extendedInfoItems = new[] { new { Key = "", Value = "" } }
                            };
                            transactions.Add(transactionEntry);

                            await InsertTransactionAsync(connection, transaction, userId, offer, initialValue, resultingValue, transactionEntry);
                            initialValue = resultingValue;

                            if (IsSpecialOffer(offer.OfferId))
                            {
                                await UpdateSpecialOfferAsync(connection, transaction, userId, offer.OfferId);
                            }
                        }

                        await UpdateCurrencyValueAsync(connection, transaction, userId, currencyType, initialValue);
                        await transaction.CommitAsync();
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }

            return (transactions, null);
        }

        private List<ItemOffer> LoadOffersFromJson(string offersDirectoryPath)
        {
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

            return allItemOffers;
        }

        private async Task<int> GetInitialCurrencyValueAsync(MySqlConnection connection, MySqlTransaction transaction, int? userId, string currencyType)
        {
            const string query = @"SELECT Value FROM userstates WHERE UserId = @UserId AND StateName = @CurrencyType";
            using (var command = new MySqlCommand(query, connection, transaction))
            {
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@CurrencyType", currencyType);

                var result = await command.ExecuteScalarAsync();
                return result != null && int.TryParse(result.ToString(), out var value) ? value : -1;
            }
        }

        private async Task InsertTransactionAsync(MySqlConnection connection, MySqlTransaction transaction, int? userId, Offer offer, int initialValue, int resultingValue, object transactionEntry)
        {
            const string query = @"
                INSERT INTO transactions
                (UserId, OfferId, InitialValue, ResultingValue, DeltaValue, OperationType, SessionId, ReferenceId, TimeStamp, DescId, StateName, StateType, OwnType)
                VALUES (@UserId, @OfferId, @InitialValue, @ResultingValue, @DeltaValue, @OperationType, @SessionId, @ReferenceId, @TimeStamp, @DescId, @StateName, @StateType, @OwnType)";

            using (var command = new MySqlCommand(query, connection, transaction))
            {
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@OfferId", offer.OfferId);
                command.Parameters.AddWithValue("@InitialValue", initialValue);
                command.Parameters.AddWithValue("@ResultingValue", resultingValue);
                command.Parameters.AddWithValue("@DeltaValue", offer.Price);
                command.Parameters.AddWithValue("@OperationType", 0);
                command.Parameters.AddWithValue("@SessionId", ((dynamic)transactionEntry).sessionId);
                command.Parameters.AddWithValue("@ReferenceId", ((dynamic)transactionEntry).referenceId);
                command.Parameters.AddWithValue("@TimeStamp", ((dynamic)transactionEntry).timeStamp);
                command.Parameters.AddWithValue("@DescId", 0);
                command.Parameters.AddWithValue("@StateName", offer.OfferId.Replace("_cr", ""));
                command.Parameters.AddWithValue("@StateType", 4);
                command.Parameters.AddWithValue("@OwnType", 2);

                await command.ExecuteNonQueryAsync();
            }
        }

        private async Task UpdateSpecialOfferAsync(MySqlConnection connection, MySqlTransaction transaction, int? userId, string offerId)
        {
            const string query = @"
                UPDATE userstates
                SET Value = 0
                WHERE UserId = @UserId AND StateName = 'class_select_token';
                INSERT INTO userstates (StateName, OwnType, Value, StateType, UserId)
                VALUES (@OfferId, 1, 1, 0, @UserId)";

            using (var command = new MySqlCommand(query, connection, transaction))
            {
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@OfferId", offerId);
                await command.ExecuteNonQueryAsync();
            }
        }

        private async Task UpdateCurrencyValueAsync(MySqlConnection connection, MySqlTransaction transaction, int? userId, string currencyType, int finalValue)
        {
            const string query = @"UPDATE userstates SET Value = @FinalValue WHERE UserId = @UserId AND StateName = @CurrencyType";

            using (var command = new MySqlCommand(query, connection, transaction))
            {
                command.Parameters.AddWithValue("@FinalValue", finalValue);
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@CurrencyType", currencyType);

                await command.ExecuteNonQueryAsync();
            }
        }

        private bool IsSpecialOffer(string offerId)
        {
            return offerId == "ranger_kit_offer" || offerId == "sniper_kit_offer" || offerId == "tactician_kit_offer";
        }
    }
}