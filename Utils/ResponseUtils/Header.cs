using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Halal_Station_Remastered.Utils.ResponseUtils
{
    public class Header
    {
        public static IActionResult AddUserContextAndReturnContent(IHeaderDictionary requestHeaders, IHeaderDictionary responseHeaders, object response)
        {
            if (requestHeaders.TryGetValue("USER_CONTEXT", out var userContextToken))
            {
                responseHeaders["USER_CONTEXT"] = userContextToken.ToString();
            }

            return new ContentResult
            {
                Content = JsonSerializer.Serialize(response),
                ContentType = "application/json"
            };
        }

        public static int? ExtractUserIdFromHeaders(IHeaderDictionary requestHeaders)
        {
            if (!requestHeaders.TryGetValue("USER_CONTEXT", out var userContextToken) || string.IsNullOrEmpty(userContextToken))
            {
                return null;
            }

            var token = userContextToken.ToString();
            var userIdString = JWTGenerator.GetUserIdFromToken(token);

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                return null;
            }

            return userId;
        }
    }
}