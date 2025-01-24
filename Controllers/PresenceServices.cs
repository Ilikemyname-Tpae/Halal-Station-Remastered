using Halal_Station_Remastered.Utils.Enums;
using Halal_Station_Remastered.Utils.Requests.PresenceServices;
using Halal_Station_Remastered.Utils.ResponseUtils;
using Halal_Station_Remastered.Utils.Services.PresenceServices;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Halal_Station_Remastered.Controllers
{
    [ApiController]
    [Route("PresenceService.svc")]
    public class PresenceServices : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public PresenceServices(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("PresenceConnect")]
        public async Task<IActionResult> PresenceConnect()
        {
            var userId = Header.ExtractUserIdFromHeaders(Request.Headers);
            var partyService = new PresenceConnectService(_configuration);

            var (partyId, matchmakeState, gameData, sessionMembers) = await partyService.GetOrCreatePartyAsync(userId);

            var response = new
            {
                PresenceConnectResult = new
                {
                    retCode = ClientCodes.Success,
                    data = new
                    {
                        Party = new
                        {
                            Id = partyId
                        },
                        SessionMembers = sessionMembers,
                        MatchmakeState = matchmakeState,
                        GameData = Convert.ToBase64String(gameData)
                    }
                }
            };
            return Header.AddUserContextAndReturnContent(Request.Headers, Response.Headers, response);
        }

        [HttpPost("ReportOnlineStats")]
        public IActionResult ReportOnlineStats()
        {
            var response = new
            {
                ReportOnlineStatsResult = new
                {
                    retCode = ClientCodes.Success,
                    data = new
                    {
                        UsersMainMenu = 1,
                        UsersQueue = 0,
                        UsersIngame = 0,
                        UsersRematch = 0,
                        MatchmakeSessions = 1
                    }
                }
            };
            return Header.AddUserContextAndReturnContent(Request.Headers, Response.Headers, response);
        }

        [HttpPost("PartyGetStatus")]
        public async Task<IActionResult> PartyGetStatus()
        {
            var userId = Header.ExtractUserIdFromHeaders(Request.Headers);
            var partyService = new PartyService(_configuration);
            var (partyId, isOwner, matchmakeState, gameData) = await partyService.GetPartyStatusAsync(userId);

            var response = new
            {
                PartyGetStatusResult = new
                {
                    retCode = ClientCodes.Success,
                    data = new
                    {
                        Party = new
                        {
                            Id = partyId
                        },
                        SessionMembers = new[]
                        {
                    new
                    {
                        User = new { Id = userId },
                        IsOwner = isOwner
                    }
                },
                        MatchmakeState = matchmakeState,
                        GameData = gameData
                    }
                }
            };
            return Header.AddUserContextAndReturnContent(Request.Headers, Response.Headers, response);
        }

        [HttpPost("PresenceGetUsersPresence")]
        public async Task<IActionResult> PresenceGetUsersPresence()
        {
            using var reader = new StreamReader(Request.Body);
            var requestBody = await reader.ReadToEndAsync();

            using var jsonDoc = JsonDocument.Parse(requestBody);
            var root = jsonDoc.RootElement;
            var usersJsonArray = root.GetProperty("users").EnumerateArray();

            var userIds = new List<int>();
            foreach (var userElement in usersJsonArray)
            {
                userIds.Add(userElement.GetProperty("Id").GetInt32());
            }

            var userPresenceService = new GetUserPresenceService(_configuration);
            var userPresences = await userPresenceService.GetUserPresencesAsync(userIds);

            var response = new
            {
                PresenceGetUsersPresenceResult = new
                {
                    retCode = ClientCodes.Success,
                    data = userPresences
                }
            };
            return Header.AddUserContextAndReturnContent(Request.Headers, Response.Headers, response);
        }

        [HttpPost("MatchmakeStart")]
        public async Task<IActionResult> MatchmakeStart()
        {
            var userId = Header.ExtractUserIdFromHeaders(Request.Headers);
            var partyService = new MatchmakeStartService(_configuration);
            var isMatchmakingStarted = await partyService.StartMatchmakingAsync(userId);

            if (isMatchmakingStarted)
            {
                var response = new
                {
                    MatchmakeStartResult = new
                    {
                        retCode = ClientCodes.Success,
                        data = true,
                    }
                };
                return Header.AddUserContextAndReturnContent(Request.Headers, Response.Headers, response);
            }
            else
            {
                var response = new
                {
                    MatchmakeStartResult = new
                    {
                        retCode = ClientCodes.CantConnectToDedicatedServer,
                        data = false,
                    }
                };
                return Header.AddUserContextAndReturnContent(Request.Headers, Response.Headers, response);
            }
        }

        [HttpPost("PartyLeave")]
        public async Task<IActionResult> PartyLeave()
        {
            var userId = Header.ExtractUserIdFromHeaders(Request.Headers);
            var partyService = new PartyLeaveService(_configuration);
            var (partyId, isOwner, matchmakeState, gameData, success) = await partyService.LeavePartyAsync(userId);

            var response = new
            {
                PartyLeaveResult = new
                {
                    retCode = ClientCodes.Success,
                    data = new
                    {
                        Party = new
                        {
                            Id = partyId
                        },
                        SessionMembers = new[]
                        {
                    new
                    {
                        User = new { Id = userId },
                        IsOwner = isOwner
                    }
                },
                        MatchmakeState = 0,
                        GameData = gameData
                    }
                }
            };
            return Header.AddUserContextAndReturnContent(Request.Headers, Response.Headers, response);
        }

        [HttpPost("MatchmakeGetStatus")]
        public async Task<IActionResult> MatchmakeGetStatus()
        {
            var matchmakingService = new MatchmakeGetStatusService(_configuration);
            var members = await matchmakingService.GetAllMembersWithMatchmakeStateAsync();
            var dedicatedServerService = new DedicatedServerInfoService(_configuration);
            var (serverId, serverAddress, transportAddress, port) = await dedicatedServerService.GetDedicatedServerInfoAsync();

            var response = new
            {
                MatchmakeGetStatusResult = new
                {
                    retCode = ClientCodes.Success,
                    data = new
                    {
                        Id = new
                        {
                            Id = serverId
                        },
                        Members = members,
                        MatchmakeTimer = 5
                    }
                }
            };
            return Header.AddUserContextAndReturnContent(Request.Headers, Response.Headers, response);
        }

        [HttpPost("GetPlaylistStat")]
        public async Task<IActionResult> GetPlaylistStat([FromBody] PlaylistRequest request)
        {
            var playlistService = new GetPlaylistStatService(_configuration);
            var playlistStats = await playlistService.GetPlaylistStatsAsync(request.Playlists.ToList());

            var response = new
            {
                GetPlaylistStatResult = new
                {
                    retCode = ClientCodes.Success,
                    data = playlistStats.ToArray()
                }
            };
            return Header.AddUserContextAndReturnContent(Request.Headers, Response.Headers, response);
        }

        [HttpPost("PartySetGameData")]
        public async Task<IActionResult> PartySetGameData()
        {
            var userId = Header.ExtractUserIdFromHeaders(Request.Headers);
            string requestBody;
            using (var reader = new StreamReader(Request.Body))
            {
                requestBody = await reader.ReadToEndAsync();
            }

            var requestData = JsonDocument.Parse(requestBody);
            if (!requestData.RootElement.TryGetProperty("gameData", out var gameDataElement) || gameDataElement.ValueKind != JsonValueKind.String)
            {
            }

            var newGameData = gameDataElement.GetString();
            var partyService = new PartySetGameDataService(_configuration);
            var success = await partyService.UpdateGameDataAsync(userId, newGameData);

            var response = new
            {
                PartySetGameDataResult = new
                {
                    retCode = ClientCodes.Success,
                    data = true,
                }
            };
            return Header.AddUserContextAndReturnContent(Request.Headers, Response.Headers, response);
        }

        [HttpPost("MatchmakeStop")]
        public async Task<IActionResult> MatchmakeStop()
        {
            var userId = Header.ExtractUserIdFromHeaders(Request.Headers);
            var partyService = new MatchmakeStopService(_configuration);
            var isMatchmakingStopped = await partyService.StopMatchmakingAsync(userId);

            if (isMatchmakingStopped)
            {
                var response = new
                {
                    MatchmakeStopResult = new
                    {
                        retCode = ClientCodes.Success,
                        data = true,
                    }
                };
                return Header.AddUserContextAndReturnContent(Request.Headers, Response.Headers, response);
            }
            else
            {
                var response = new
                {
                    MatchmakeStopResult = new
                    {
                        retCode = ClientCodes.CantConnectToDedicatedServer,
                        data = false,
                    }
                };
                return Header.AddUserContextAndReturnContent(Request.Headers, Response.Headers, response);
            }
        }
    }
}