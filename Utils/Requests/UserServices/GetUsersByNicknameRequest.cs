namespace Halal_Station_Remastered.Utils.Requests.UserServices
{
    public class GetUsersByNicknameRequest
    {
        public string NicknamePrefix { get; set; }
        public int MaxResults { get; set; } = 16;
    }
}