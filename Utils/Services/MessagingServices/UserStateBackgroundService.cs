using Halal_Station_Remastered.Utils.Services.MessagingServices;
using MySql.Data.MySqlClient;

public class UserStateUpdaterServices
{
    private readonly string _connectionString;
    private readonly Dictionary<int?, DateTime> _userLastRequestTimes = new();
    private readonly Dictionary<int?, HashSet<int>> _userSeenMessages = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(4);

    public UserStateUpdaterServices(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
        StartBackgroundTask();
    }

    public void UpdateUserRequestTime(int? userId)
    {
        lock (_userLastRequestTimes)
        {
            _userLastRequestTimes[userId] = DateTime.UtcNow;
        }
    }

    public void MarkMessageAsSeen(int? userId, int messageId)
    {
        lock (_userSeenMessages)
        {
            if (!_userSeenMessages.ContainsKey(userId))
            {
                _userSeenMessages[userId] = new HashSet<int>();
            }
            _userSeenMessages[userId].Add(messageId);
        }
    }

    private void StartBackgroundTask()
    {
        Task.Run(async () =>
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                CheckAndUpdateMatchmakeState();
                await Task.Delay(_checkInterval, _cancellationTokenSource.Token);
            }
        }, _cancellationTokenSource.Token);
    }

    private void CheckAndUpdateMatchmakeState()
    {
        List<int?> userIdsToUpdate = new();

        lock (_userLastRequestTimes)
        {
            var currentTime = DateTime.UtcNow;

            foreach (var entry in _userLastRequestTimes)
            {
                if ((currentTime - entry.Value).TotalSeconds > 10)
                {
                    userIdsToUpdate.Add(entry.Key);
                }
            }
        }

        if (userIdsToUpdate.Count > 0)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                foreach (var userId in userIdsToUpdate)
                {
                    var updatePartyQuery = "UPDATE party SET MatchmakeState = 0 WHERE UserId = @UserId";
                    var updatePartyCommand = new MySqlCommand(updatePartyQuery, connection);
                    updatePartyCommand.Parameters.AddWithValue("@UserId", userId);
                    var rowsAffectedParty = updatePartyCommand.ExecuteNonQuery();

                    var updateUserQuery = "UPDATE users SET State = 0, IsInvitable = 0 WHERE UserId = @UserId";
                    var updateUserCommand = new MySqlCommand(updateUserQuery, connection);
                    updateUserCommand.Parameters.AddWithValue("@UserId", userId);
                    var rowsAffectedUser = updateUserCommand.ExecuteNonQuery();

                    if (rowsAffectedParty > 0 || rowsAffectedUser > 0)
                    {
                        lock (_userLastRequestTimes)
                        {
                            _userLastRequestTimes.Remove(userId);
                        }
                    }
                }
            }
        }
    }

    public IEnumerable<Message> FilterSeenMessages(int? userId, IEnumerable<Message> messages)
    {
        lock (_userSeenMessages)
        {
            if (!_userSeenMessages.ContainsKey(userId))
            {
                return messages;
            }

            var seenMessages = _userSeenMessages[userId];
            return messages.Where(msg => !seenMessages.Contains(msg.Id)).ToList();
        }
    }

    public void StopBackgroundTask()
    {
        _cancellationTokenSource.Cancel();
    }
}