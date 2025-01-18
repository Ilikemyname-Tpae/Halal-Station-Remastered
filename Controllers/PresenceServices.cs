using Halal_Station_Remastered.Utils.Enums;
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
    }
}