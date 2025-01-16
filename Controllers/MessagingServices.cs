using Halal_Station_Remastered.Utils.Enums;
using Halal_Station_Remastered.Utils.Requests.MessagingServices;
using Halal_Station_Remastered.Utils.ResponseUtils;
using Halal_Station_Remastered.Utils.Services.MessagingServices;
using Microsoft.AspNetCore.Mvc;

namespace Halal_Station_Remastered.Controllers
{
    [ApiController]
    [Route("MessagingService.svc")]
    public class MessagingServices : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public MessagingServices(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("JoinChannels")]
        public async Task<IActionResult> JoinChannels([FromBody] JoinChannelsRequest request)
        {
            var userId = Header.ExtractUserIdFromHeaders(Request.Headers);
            var joinService = new JoinChannelsService(_configuration);

            await joinService.AddUserToChannelsAsync(userId, request.ChannelNames);

            var response = new
            {
                JoinChannelsResult = new
                {
                    retCode = ClientCodes.Success,
                    data = request.ChannelNames.Select(name => new
                    {
                        Name = name,
                        Version = 1,
                        Messages = new List<object>(),
                        Members = new List<object> { new { Id = userId } }
                    }).ToList()
                }
            };
            return Header.AddUserContextAndReturnContent(Request.Headers, Response.Headers, response);
        }

        [HttpPost("LeaveChannels")]
        public async Task<IActionResult> LeaveChannels([FromBody] LeaveChannelsRequest request)
        {
            var userId = Header.ExtractUserIdFromHeaders(Request.Headers);
            var leaveService = new LeaveChannelsService(_configuration);

            var remainingChannels = await leaveService.RemoveUserFromChannelsAsync(userId, request.ChannelNames);

            var response = new
            {
                LeaveChannelsResult = new
                {
                    retCode = ClientCodes.Success,
                    data = remainingChannels
                }
            };
            return Header.AddUserContextAndReturnContent(Request.Headers, Response.Headers, response);
        }
    }
}