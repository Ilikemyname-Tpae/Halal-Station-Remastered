using Halal_Station_Remastered.Utils.Enums;
using Halal_Station_Remastered.Utils.Requests.SessionControlServices;
using Halal_Station_Remastered.Utils.ResponseUtils;
using Halal_Station_Remastered.Utils.Services.SessionControlServices;
using Microsoft.AspNetCore.Mvc;

namespace Halal_Station_Remastered.Controllers
{
    [ApiController]
    [Route("SessionControlService.svc")]
    public class SessionControlServices : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public SessionControlServices(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("ClientGetStatus")]
        public async Task<IActionResult> ClientGetStatus()
        {
            var dedicatedServerService = new DedicatedServerInfoService(_configuration);
            var (serverId, serverAddress, transportAddress, port) = await dedicatedServerService.GetDedicatedServerInfoAsync();

            var response = new
            {
                ClientGetStatusResult = new
                {
                    retCode = ClientCodes.Success,
                    data = new
                    {
                        Game = new
                        {
                            Id = serverId
                        },
                        DedicatedServer = new
                        {
                            ServerID = serverAddress,                        
                            ServerAddr = transportAddress,
                            Port = port
                        }
                    }
                }
            };
            return Header.AddUserContextAndReturnContent(Request.Headers, Response.Headers, response);
        }

        [HttpPost("GetSessionUsers")]
        public async Task<IActionResult> GetSessionUsers()
        {
            var matchmakingService = new GetSessionUsersService(_configuration);
            var userIds = await matchmakingService.GetUserIdsWithMatchmakeStateAsync();

            var response = new
            {
                GetSessionUsersResult = new
                {
                    retCode = ClientCodes.Success,
                    data = new
                    {
                        UserIds = userIds
                    }
                }
            };
            return Header.AddUserContextAndReturnContent(Request.Headers, Response.Headers, response);
        }

        [HttpPost("DedicatedServerInfo")]
        public async Task<IActionResult> DedicatedServerInfo([FromBody] DedicatedServerInfoRequest request)
        {
            var dedicatedServerService = new DedicatedServerInfoService(_configuration);
			
            await dedicatedServerService.AddDedicatedServerAsync(
                request.SecureID,
                request.SecureAddress,
                request.TransportAddress,
                11774
            );

            var response = new
            {
                DedicatedServerInfoResult = new
                {
                    retCode = ClientCodes.Success,
                    data = true
                }
            };
           return Header.AddUserContextAndReturnContent(Request.Headers, Response.Headers, response);
        }
    }
}