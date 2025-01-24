using Halal_Station_Remastered.Utils.Enums;
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
        public IActionResult ClientGetStatus()
        {
            var response = new
            {
                ClientGetStatusResult = new
                {
                    retCode = ClientCodes.Success,
                    data = new
                    {
                        Game = new
                        {
                        },
                        DedicatedServer = new
                        {
                            ServerAddr = "000.00.000.0",
                            Port = 11774
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
    }
}