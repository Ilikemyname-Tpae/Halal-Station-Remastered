using MySql.Data.MySqlClient;
using System.Text;

namespace Halal_Station_Remastered.Utils.Services.PresenceServices
{
    public class GetPlaylistStatService
    {
        private static string _connectionString;

        public GetPlaylistStatService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<List<object>> GetPlaylistStatsAsync(List<string> playlists)
        {
            var rows = new List<(string GameData, int MatchmakeState)>();
            var playlistStats = new List<object>();

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                const string query = "SELECT GameData, MatchmakeState FROM party";
                using (var command = new MySqlCommand(query, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            string base64GameData = reader.GetString(reader.GetOrdinal("GameData"));
                            int matchmakeState = reader.GetInt32(reader.GetOrdinal("MatchmakeState"));
                            rows.Add((base64GameData, matchmakeState));
                        }
                    }
                }
            }

            foreach (var playlist in playlists)
            {
                int playersNumber = 0;

                foreach (var row in rows)
                {
                    string decodedGameData = Encoding.UTF8.GetString(Convert.FromBase64String(row.GameData));

                    if (decodedGameData.Contains(playlist) && row.MatchmakeState > 0)
                    {
                        playersNumber++;
                    }
                }

                playlistStats.Add(new
                {
                    Playlist = playlist,
                    PlayersNumber = playersNumber
                });
            }
            return playlistStats;
        }
    }
}