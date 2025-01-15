using Halal_Station_Remastered.Utils.JTW;
using Halal_Station_Remastered.Utils.ResponseUtils;

namespace Halal_Station_Remastered.Utils.Requests.AuthorizationService
{
    public class SignInRequest
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public string Provider { get; set; }
        public Signature Signature { get; set; }
        public List<ServiceVersion> Versions { get; set; }
    }
}