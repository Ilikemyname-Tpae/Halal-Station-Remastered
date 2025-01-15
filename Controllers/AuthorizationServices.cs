using Microsoft.AspNetCore.Mvc;
using Halal_Station_Remastered.Utils.Enums;
using Halal_Station_Remastered.Utils.Requests.AuthorizationService;
using Halal_Station_Remastered.Utils.Services.AuthorizationServices;
using Halal_Station_Remastered.Utils.ResponseUtils;
using Halal_Station_Remastered.Utils.Authorization;

namespace Halal_Station_Remastered.Controllers
{
    [ApiController]
    [Route("AuthorizationService.svc")]
    public class RegistrationController : ControllerBase
    {
        private readonly AuthorizationUser _authorizationUser;

        public RegistrationController(IConfiguration configuration)
        {
            _authorizationUser = new AuthorizationUser(configuration);
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Username and password are required.");

            if (await _authorizationUser.UserExists(request.Username))
                return BadRequest(new { ErrorCode = ClientCodes.NicknameChangeNotAvailable });

            await _authorizationUser.CreateUser(request.Username, request.Password);
            return Ok("Spartan has registered successfully!");
        }

        private static readonly Dictionary<string, string> _tokenStore = new Dictionary<string, string>();

        [HttpPost("SignIn")]
        public async Task<IActionResult> SignIn([FromBody] SignInRequest request, IConfiguration configuration)
        {
            var titleservers = configuration.GetSection("TitleServers").Get<List<ServerInfo>>();
            var diagnosticservices = configuration.GetSection("DiagnosticsServices").Get<List<ServerInfo>>();
            var user = await _authorizationUser.GetUser(request.Login);

            if (user.Password == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                return Unauthorized(new { ErrorCode = ClientCodes.InvalidLoginOrPassword });
            }

            var token = JWTGenerator.GenerateJwtToken(user.UserId.ToString(), request.Login);
            var response = new
            {
                SignInResult = new
                {
                    retCode = ClientCodes.Success,
                    data = new
                    {
                        AuthorizationToken = token,
                        TitleServers = titleservers,
                        DiagnosticServices = diagnosticservices
                    }
                }
            };
            return Header.AddUserContextAndReturnContent(Request.Headers, Response.Headers, response);
        }
    }
}