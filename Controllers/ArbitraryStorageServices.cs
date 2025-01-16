using Halal_Station_Remastered.Utils.Enums;
using Halal_Station_Remastered.Utils.ResponseUtils;
using Microsoft.AspNetCore.Mvc;

namespace Halal_Station_Remastered.Controllers
{
    [ApiController]
    [Route("ArbitraryStorageService.svc")]
    public class ArbitraryStorageServices : ControllerBase
    {
        [HttpGet("WriteDiagnosticsData")]
        public IActionResult WriteDiagnosticsData()
        {
            var response = new
            {
                WriteDiagnosticsDataResult = new
                {
                    retCode = ClientCodes.Success,
                    data = true
                }
            };
            return Header.AddUserContextAndReturnContent(Request.Headers, Response.Headers, response);
        }

        [HttpPost("WriteADFPack")]
        public IActionResult WriteADFPack()
        {
            var response = new
            {
                WriteADFPackResult = new
                {
                    retCode = ClientCodes.Success,
                    data = true
                }
            };
            return Header.AddUserContextAndReturnContent(Request.Headers, Response.Headers, response);
        }
    }
}