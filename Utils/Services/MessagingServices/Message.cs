namespace Halal_Station_Remastered.Utils.Services.MessagingServices
{
    public class Message
    {
        public int FromId { get; set; }
        public string Text { get; set; }
        public long Timestamp { get; set; }
        public int Id { get; internal set; }
    }
}