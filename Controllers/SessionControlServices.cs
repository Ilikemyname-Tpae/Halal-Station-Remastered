using Halal_Station_Remastered.Utils.Enums;
using Halal_Station_Remastered.Utils.ResponseUtils;
using Microsoft.AspNetCore.Mvc;

namespace Halal_Station_Remastered.Controllers
{
    [ApiController]
    [Route("SessionControlService.svc")]
    public class SessionControlServices : ControllerBase
    {
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
                            Id = "f6cd36b5-39aa-46af-b7ca-0d15b69e7107"
                        },
                        DedicatedServer = new
                        {
                            ServerID = "00000000-0000-0000-0000-000000000000",
                            ServerAddr = "000.00.000.0",
                            Port = 11774
                        }
                    }
                }
            };
            return Header.AddUserContextAndReturnContent(Request.Headers, Response.Headers, response);
        }
    }
}