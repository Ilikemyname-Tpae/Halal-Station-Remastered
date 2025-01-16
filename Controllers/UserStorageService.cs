using Halal_Station_Remastered.Utils.Enums;
using Halal_Station_Remastered.Utils.ResponseUtils;
using Halal_Station_Remastered.Utils.Services.UserStorageServices;
using Microsoft.AspNetCore.Mvc;

namespace Halal_Station_Remastered.Controllers
{
    [ApiController]
    [Route("UserStorageService.svc")]
    public class UserStorageServices : ControllerBase
    {
        private readonly UserPrivateDataService _userPrivateDataService;

        public UserStorageServices(IConfiguration configuration)
        {
            _userPrivateDataService = new UserPrivateDataService(configuration);
        }

        [HttpPost("GetPrivateData")]
        public async Task<IActionResult> GetPrivateData()
        {
            var userId = Header.ExtractUserIdFromHeaders(Request.Headers);

            var (layout, version, data) = await _userPrivateDataService.GetUserPrivateDataAsync(userId);

            var response = new
            {
                GetPrivateDataResult = new
                {
                    retCode = ClientCodes.Success,
                    data = new
                    {
                        Layout = layout,
                        Version = version,
                        Data = data
                    }
                }
            };
            return Header.AddUserContextAndReturnContent(Request.Headers, Response.Headers, response);
        }
    }
}