using Halal_Station_Remastered.Utils.Enums;
using Halal_Station_Remastered.Utils.ResponseUtils;
using Halal_Station_Remastered.Utils.Services.UserServices;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Halal_Station_Remastered.Controllers
{
    [ApiController]
    [Route("UserService.svc")]
    public class UserServices : ControllerBase
    {
        [HttpPost("GetShop")]
        public IActionResult GetShop()
        {
            var shopDirectoryPath = Path.Combine(AppContext.BaseDirectory, "JsonData", "Shop");

            var allShops = Directory.Exists(shopDirectoryPath)
                ? Directory.GetFiles(shopDirectoryPath, "*.json", SearchOption.AllDirectories)
                    .Select(System.IO.File.ReadAllText)
                    .Select(json => JsonSerializer.Deserialize<ShopData>(json))
                    .Where(shop => shop != null)
                    .ToList()
                : new List<ShopData>();

            var response = new
            {
                GetShopResult = new
                {
                    retCode = ClientCodes.Success,
                    data = allShops
                }
            };

            return Header.AddUserContextAndReturnContent(Request.Headers, Response.Headers, response);
        }
    }
}