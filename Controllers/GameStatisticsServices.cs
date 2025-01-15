using Halal_Station_Remastered.Utils.Enums;
using Halal_Station_Remastered.Utils.ResponseUtils;
using Microsoft.AspNetCore.Mvc;

namespace Halal_Station_Remastered.Controllers
{
    [ApiController]
    [Route("GameStatisticsService.svc")]
    public class GameStatisticServices : ControllerBase
    {
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
    }
}