using Halal_Station_Remastered.Utils.Enums;
using Halal_Station_Remastered.Utils.ResponseUtils;
using Halal_Station_Remastered.Utils.Services.UserServices;
using Halal_Station_Remastered.Utils.Services.UserStorageServices;
using Microsoft.AspNetCore.Mvc;
using System.Text;
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

        [HttpPost("SetPrivateData")]
        public async Task<IActionResult> SetPrivateData()
        {
            var userId = Header.ExtractUserIdFromHeaders(Request.Headers);
            string requestBody;
            using (var reader = new StreamReader(Request.Body))
            {
                requestBody = await reader.ReadToEndAsync();
            }

            var jsonStartIndex = requestBody.IndexOf('{');
            var jsonString = requestBody.Substring(jsonStartIndex);
            var jsonDoc = JsonDocument.Parse(jsonString);

            if (jsonDoc.RootElement.TryGetProperty("data", out JsonElement dataElement))
            {
                var preferences = dataElement.GetRawText();

                var userPrivateService = new UserSetPrivateDataService(_configuration);
                await userPrivateService.SetPreferencesAsync(userId, preferences);
            }

            var response = new
            {
                SetPrivateDataResult = new
                {
                    retCode = ClientCodes.Success,
                    data = true,
                }
            };
            return Header.AddUserContextAndReturnContent(Request.Headers, Response.Headers, response);
        }

        [HttpPost("SetPublicData")]
        public async Task<IActionResult> SetPublicData()
        {
            var userId = Header.ExtractUserIdFromHeaders(Request.Headers);

            string requestBody;
            using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                requestBody = await reader.ReadToEndAsync();
            }

            var requestData = JsonSerializer.Deserialize<Dictionary<string, object>>(requestBody);
            if (requestData == null || !requestData.TryGetValue("containerName", out var containerNameObj) || !requestData.TryGetValue("data", out var dataObj))
            {
                return BadRequest("bad request data");
            }

            string containerName = containerNameObj.ToString();
            var data = dataObj as JsonElement?;

            string armorLoadout = null, weaponLoadout = null, customization = null;

            switch (containerName)
            {
                case "armor_loadouts":
                    armorLoadout = data?.ToString();
                    break;
                case "weapon_loadouts":
                    weaponLoadout = data?.ToString();
                    break;
                case "customization":
                    customization = data?.ToString();
                    break;
                default:
                    return BadRequest("unsupported container");
            }

            var userLoadoutService = new SetPublicDataService(_configuration);
            await userLoadoutService.UpdateUserLoadoutAsync(userId, armorLoadout, weaponLoadout, customization);

            var response = new
            {
                SetPublicDataResult = new
                {
                    retCode = ClientCodes.Success,
                    data = new
                    {
                        ArmorLoadout = armorLoadout,
                        Customization = customization,
                        WeaponLoadout = weaponLoadout,
                    }
                }
            };
            return Header.AddUserContextAndReturnContent(Request.Headers, Response.Headers, response);
        }
    }
}