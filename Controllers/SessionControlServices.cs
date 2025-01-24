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
                            Id = "cda60f93-18d1-41a6-9908-5dd962b898da"
                        },
                        DedicatedServer = new
                        {
                            ServerID = "143b1a10-85e4-4190-9694-41bb7ad92727",
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