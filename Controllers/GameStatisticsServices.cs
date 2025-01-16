using Halal_Station_Remastered.Utils.Enums;
using Halal_Station_Remastered.Utils.ResponseUtils;
using Halal_Station_Remastered.Utils.Services.GameStatisticServices;
using Microsoft.AspNetCore.Mvc;

namespace Halal_Station_Remastered.Controllers
{
    [ApiController]
    [Route("GameStatisticsService.svc")]
    public class GameStatisticServices : ControllerBase
    {
        private readonly GetUserChallengesService _userChallengesService;

        public GameStatisticServices(IConfiguration configuration)
        {
            _userChallengesService = new GetUserChallengesService(configuration);
        }

        [HttpPost("CheckNewUserChallenges")]
        public IActionResult CheckNewUserChallenges()
        {
            var response = new
            {
                CheckNewUserChallengesResult = new
                {
                    retCode = ClientCodes.Success,
                    data = true
                }
            };
            return Header.AddUserContextAndReturnContent(Request.Headers, Response.Headers, response);
        }

        [HttpPost("GetUserChallenges")]
        public async Task<IActionResult> GetUserChallenges()
        {
            var userId = Header.ExtractUserIdFromHeaders(Request.Headers);
            var challenges = await _userChallengesService.GetChallengesAsync(userId);

            var response = new
            {
                GetUserChallengesResult = new
                {
                    retCode = ClientCodes.Success,
                    data = new ChallengeData
                    {
                        Version = 0,
                        Challenges = challenges
                    }
                }
            };
            return Header.AddUserContextAndReturnContent(Request.Headers, Response.Headers, response);
        }
    }
}