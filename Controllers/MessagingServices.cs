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
        private readonly UserStateUpdaterServices _userStateUpdaterServices;

        public MessagingServices(IConfiguration configuration, UserStateUpdaterServices userstateupdaterservices)
        {
            _configuration = configuration;
            _userStateUpdaterServices = userstateupdaterservices;
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

        [HttpPost("Receive")]
        public async Task<IActionResult> Receive([FromBody] ReceiveRequest request)
        {
            var userId = Header.ExtractUserIdFromHeaders(Request.Headers);
            _userStateUpdaterServices.UpdateUserRequestTime(userId);

            var channelService = new ChannelService(_configuration, _userStateUpdaterServices);
            var channelsData = await channelService.GetChannelDataAsync(request.Channels, userId);

            var response = new
            {
                ReceiveResult = new
                {
                    retCode = ClientCodes.Success,
                    data = channelsData
                }
            };
            return Header.AddUserContextAndReturnContent(Request.Headers, Response.Headers, response);
        }

        [HttpPost("Send")]
        public async Task<IActionResult> Send([FromBody] SendRequest request)
        {
            var userId = Header.ExtractUserIdFromHeaders(Request.Headers);
            var channelService = new SendService(_configuration);
            var isSuccess = await channelService.AddMessageToChannelAsync(userId, request.ChannelName, request.Message);

            if (!isSuccess)
            {
                return NotFound("Channel not found.");
            }

            var response = new
            {
                SendResult = new
                {
                    retCode = ClientCodes.Success,
                    data = true
                }
            };
            return Header.AddUserContextAndReturnContent(Request.Headers, Response.Headers, response);
        }
    }
}