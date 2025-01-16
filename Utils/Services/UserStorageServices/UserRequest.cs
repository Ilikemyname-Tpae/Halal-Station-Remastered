using Halal_Station_Remastered.Utils.Services.UserServices;

namespace Halal_Station_Remastered.Utils.Services.UserStorageServices
{
    public class UserRequest
    {
        public List<UserId> Users { get; set; }
        public string ContainerName { get; set; }
    }
}
