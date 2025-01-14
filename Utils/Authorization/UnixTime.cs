namespace Halal_Station_Remastered.Utils.Authorization
{
    public class UnixTime
    {
        public static long GetNow()
        {
            DateTimeOffset dateTimeOffset = new DateTimeOffset(DateTime.UtcNow, TimeSpan.Zero);
            return dateTimeOffset.ToUnixTimeSeconds();
        }
    }
}