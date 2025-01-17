using Halal_Station_Remastered.Utils.Enums;
using Halal_Station_Remastered.Utils.ResponseUtils;
using Halal_Station_Remastered.Utils.Services.UserServices;
using Halal_Station_Remastered.Utils.Services.UserStorageServices;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Halal_Station_Remastered.Controllers
{
    [ApiController]
    [Route("UserStorageService.svc")]
    public class UserStorageServices : ControllerBase
    {
        private readonly UserPrivateDataService _userPrivateDataService;
        private readonly IConfiguration _configuration;
        public UserStorageServices(IConfiguration configuration)
        {
            _configuration = configuration;
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

        [HttpPost("GetPublicData")]
        public async Task<IActionResult> GetPublicData()
        {
            string requestBody;
            using (var reader = new StreamReader(Request.Body))
            {
                requestBody = await reader.ReadToEndAsync();
            }

            UserRequest requestData;

            using (var jsonDoc = JsonDocument.Parse(requestBody))
            {
                var root = jsonDoc.RootElement;

                if (!root.TryGetProperty("containerName", out var containerNameElement) ||
                    !root.TryGetProperty("users", out var usersElement) ||
                    usersElement.GetArrayLength() == 0)
                {
                    return BadRequest("Request must contain 'containerName' and 'users'.");
                }

                var containerName = containerNameElement.GetString();
                var userIds = usersElement.EnumerateArray()
                    .Select(u => u.GetProperty("Id").GetInt32())
                    .ToList();

                requestData = new UserRequest
                {
                    Users = userIds.Select(id => new UserId { Id = id }).ToList(),
                    ContainerName = containerName
                };
            }

            var publicDataService = new GetPublicDataService(_configuration);
            var responseDataList = await publicDataService.GetPublicDataAsync(requestData);

            var response = new
            {
                GetPublicDataResult = new
                {
                    retCode = ClientCodes.Success,
                    data = responseDataList
                }
            };

            return Header.AddUserContextAndReturnContent(Request.Headers, Response.Headers, response);
        }
    }
}