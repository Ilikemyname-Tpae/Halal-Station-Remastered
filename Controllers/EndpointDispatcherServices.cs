using Halal_Station_Remastered.Utils.Authorization;
using Halal_Station_Remastered.Utils.ResponseUtils;
using Microsoft.AspNetCore.Mvc;

namespace Halal_Station_Remastered.Controllers
{
    [ApiController]
    [Route("EndpointsDispatcherService.svc")]
    public class EndpointDispatcherServices : ControllerBase
    {
        [HttpPost("GetAuthorizationEndpointsAndDate")]
        public IActionResult GetAuthorizationEndpointsAndDate(IConfiguration configuration)
        {
            var endpoints = configuration.GetSection("AuthorizationEndpoints").Get<List<ServerInfo>>();

            var response = new
            {
                GetAuthorizationEndpointsAndDateResult = new
                {
                    retCode = 0,
                    data = new
                    {
                        Endpoints = endpoints,
                        DateTime = UnixTime.GetNow()
                    }
                }
            };
            return Header.AddUserContextAndReturnContent(Request.Headers, Response.Headers, response);
        }
    }
}