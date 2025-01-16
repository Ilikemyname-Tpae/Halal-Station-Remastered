using Halal_Station_Remastered.Utils.Enums;
using Halal_Station_Remastered.Utils.ResponseUtils;
using Halal_Station_Remastered.Utils.Services.FriendsServices;
using Halal_Station_Remastered.Utils.Services.PresenceServices;
using Microsoft.AspNetCore.Mvc;

namespace Halal_Station_Remastered.Controllers
{
    [ApiController]
    [Route("FriendsService.svc")]
    public class FriendsServices : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public FriendsServices(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("GetSubscriptions")]
        public async Task<IActionResult> GetSubscriptions()
        {
            var userId = Header.ExtractUserIdFromHeaders(Request.Headers);
            var subscriptionService = new GetSubscriptionsService(_configuration);
            var userList = await subscriptionService.GetUserSubscriptionsAsync(userId);
            var response = new
            {
                GetSubscriptionsResult = new
                {
                    retCode = ClientCodes.Success,
                    data = new[]
                    {
                        new
                        {
                            User = new { Id = userId },
                            Version = 0,
                            Subscriptions = new { UserList = userList }
                        }
                    }
                }
            };
            return Header.AddUserContextAndReturnContent(Request.Headers, Response.Headers, response);
        }
    }
}