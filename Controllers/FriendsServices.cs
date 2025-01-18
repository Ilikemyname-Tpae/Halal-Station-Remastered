using Halal_Station_Remastered.Utils.Enums;
using Halal_Station_Remastered.Utils.ResponseUtils;
using Halal_Station_Remastered.Utils.Services.FriendsServices;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

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

        [HttpPost("SubscriptionRemove")]
        public async Task<IActionResult> SubscriptionRemove([FromBody] JsonElement requestBody)
        {
            var userId = Header.ExtractUserIdFromHeaders(Request.Headers);
            var subscriptionService = new RemoveAddSubscriptionService(_configuration);

            if (!requestBody.TryGetProperty("userId", out var userIdElement) ||
                !userIdElement.TryGetProperty("Id", out var idElement) ||
                !idElement.TryGetInt32(out var userIdToRemove))
            {
                return BadRequest("");
            }

            var updatedUserList = await subscriptionService.RemoveSubscriptionAsync(userId, userIdToRemove);

            var response = new
            {
                SubscriptionRemoveResult = new
                {
                    retCode = ClientCodes.Success,
                    data = new[]
                    {
                new
                {
                    User = new { Id = userId },
                    Version = 0,
                    Subscriptions = new { UserList = updatedUserList }
                }
            }
                }
            };
            return Header.AddUserContextAndReturnContent(Request.Headers, Response.Headers, response);
        }

        [HttpPost("SubscriptionAdd")]
        public async Task<IActionResult> SubscriptionAdd([FromBody] JsonElement requestBody)
        {
            var userId = Header.ExtractUserIdFromHeaders(Request.Headers);
            if (!requestBody.TryGetProperty("userId", out var userIdElement) ||
                !userIdElement.TryGetProperty("Id", out var idElement) ||
                !idElement.TryGetInt32(out var userIdToAdd))
            {
                return BadRequest("Invalid request body.");
            }

            var subscriptionService = new RemoveAddSubscriptionService(_configuration);
            var updatedUserList = await subscriptionService.AddSubscriptionAsync(userId, userIdToAdd);

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
                    Subscriptions = new { UserList = updatedUserList }
                }
            }
                }
            };
            return Header.AddUserContextAndReturnContent(Request.Headers, Response.Headers, response);
        }
    }
}