using Halal_Station_Remastered.Utils.Enums;
using Halal_Station_Remastered.Utils.ResponseUtils;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Halal_Station_Remastered.Controllers
{
    [ApiController]
    [Route("TitleResourceService.svc")]
    public class TitleResourceServices : ControllerBase
    {
        private static readonly string InstancesFolder = Path.Combine(AppContext.BaseDirectory, "JsonData\\TitleInstances");

        [HttpPost("GetTitleConfiguration")]
        public IActionResult GetTitleConfiguration()
        {
            var combinedInstances = Directory.GetFiles(InstancesFolder, "*.json", SearchOption.AllDirectories)
                                              .SelectMany(filePath => JsonSerializer.Deserialize<List<dynamic>>(System.IO.File.ReadAllText(filePath)))
                                              .ToList();

            var response = new
            {
                GetTitleConfigurationResult = new
                {
                    retCode = ClientCodes.Success,
                    data = new { combinationHash = "", instances = combinedInstances }
                }
            };
            return Header.AddUserContextAndReturnContent(Request.Headers, Response.Headers, response);
        }

        [HttpPost("GetTitleTagsPatchConfiguration")]
        public IActionResult GetTitleTagsPatchConfiguration()
        {
            var response = new
            {
                GetTitleTagsPatchConfigurationResult = new
                {
                    retCode = ClientCodes.Success,
                    data = new
                    {
                        CombinationHash = "",
                        Tags = new[] { new { Name = "", Type = 0, StrVal = "", IntVal = 0, FloatVal = 0.0 } } // genuinely no clue what the purpose of GetTitleTagsPatchConfiguration is, saber COUlD'VE done this when they needed to change shit quick whilst everything was live.
                    }
                }
            };
            return Header.AddUserContextAndReturnContent(Request.Headers, Response.Headers, response);
        }
    }
}