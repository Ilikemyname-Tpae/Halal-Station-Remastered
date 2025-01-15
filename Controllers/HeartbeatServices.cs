using Halal_Station_Remastered.Utils.Authorization;
using Halal_Station_Remastered.Utils.Enums;
using Halal_Station_Remastered.Utils.ResponseUtils;
using Microsoft.AspNetCore.Mvc;

namespace Halal_Station_Remastered.Controllers
{
    [ApiController]
    [Route("HeartbeatService.svc")]
    public class HeartbeatServices : ControllerBase
    {
        [HttpPost("GetServicesStatus")]
        public IActionResult GetServicesStatus(IConfiguration configuration)
        {
            var services = configuration.GetSection("ServicesStatus").Get<List<ServicesStatus>>();

            var response = new
            {
                GetServicesStatusResult = new
                {
                    retCode = ClientCodes.Success,
                    data = services
                }
            };
            return Header.AddUserContextAndReturnContent(Request.Headers, Response.Headers, response);
        }
    }
}