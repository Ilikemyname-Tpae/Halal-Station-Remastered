using Halal_Station_Remastered.Utils.Requests.MessagingServices;
using MySql.Data.MySqlClient;
using System.Data;
using System.Text.Json;

namespace Halal_Station_Remastered.Utils.Services.MessagingServices
{
    public class ChannelService
    {
        private readonly string _connectionString;
        private readonly UserStateUpdaterServices _userStateUpdaterServices;

        public ChannelService(IConfiguration configuration, UserStateUpdaterServices userStateUpdaterServices)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _userStateUpdaterServices = userStateUpdaterServices;
        }

        public async Task<List<object>> GetChannelDataAsync(List<ChannelRequest> channels, int? userId)
        {
            var channelsData = new List<object>();

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                foreach (var channel in channels)
                {
                    var selectQuery = "SELECT Id, Name, Messages, Members FROM Channels WHERE Name = @Name OR Name = @PrivateName";
                    var selectCommand = new MySqlCommand(selectQuery, connection);
                    selectCommand.Parameters.AddWithValue("@Name", channel.Name);
                    selectCommand.Parameters.AddWithValue("@PrivateName", $"#private_{channel.Name}");

                    using (var reader = await selectCommand.ExecuteReaderAsync())
                    {
                        if (reader.Read())
                        {
                            int channelId = reader.GetInt32("Id");
                            var channelName = reader.GetString("Name");
                            var messages = reader.GetString("Messages");
                            var members = reader.GetString("Members");

                            using JsonDocument doc = JsonDocument.Parse(messages);
                            var messageArray = doc.RootElement;

                            var messageList = new List<Message>();
                            foreach (JsonElement msgElement in messageArray.EnumerateArray())
                            {
                                var fromElement = msgElement.GetProperty("From");
                                messageList.Add(new Message
                                {
                                    FromId = fromElement.GetProperty("Id").GetInt32(),
                                    Text = msgElement.GetProperty("Text").GetString(),
                                    Timestamp = msgElement.GetProperty("Timestamp").GetInt64(),
                                    Id = messageList.Count + 1
                                });
                            }

                            var filteredMessages = _userStateUpdaterServices.FilterSeenMessages(userId, messageList);

                            foreach (var message in filteredMessages)
                            {
                                _userStateUpdaterServices.MarkMessageAsSeen(userId, message.Id);
                            }

                            var memberList = JsonSerializer.Deserialize<List<int>>(members);

                            channelsData.Add(new
                            {
                                Id = channelId,
                                Name = channelName,
                                Version = 1,
                                Messages = filteredMessages.Select(msg => new
                                {
                                    From = new { Id = msg.FromId },
                                    Text = msg.Text,
                                    Timestamp = msg.Timestamp
                                }).ToList(),
                                Members = memberList.Select(memberId => new { Id = memberId }).ToList()
                            });
                        }
                    }
                }

                if (userId.HasValue)
                {
                    var updateQuery = "UPDATE users SET State = @State, IsInvitable = @IsInvitable WHERE UserId = @UserId";
                    using (var updateCommand = new MySqlCommand(updateQuery, connection))
                    {
                        updateCommand.Parameters.AddWithValue("@State", 1);
                        updateCommand.Parameters.AddWithValue("@IsInvitable", 1);
                        updateCommand.Parameters.AddWithValue("@UserId", userId.Value);

                        await updateCommand.ExecuteNonQueryAsync();
                    }
                }
            }

            return channelsData;
        }
    }
}