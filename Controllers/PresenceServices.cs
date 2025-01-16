using Halal_Station_Remastered.Utils.Enums;
using Halal_Station_Remastered.Utils.ResponseUtils;
using Halal_Station_Remastered.Utils.Services.PresenceServices;
using Microsoft.AspNetCore.Mvc;

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
    }
}