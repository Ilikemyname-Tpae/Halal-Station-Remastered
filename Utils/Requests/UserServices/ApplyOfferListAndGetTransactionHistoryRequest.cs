namespace Halal_Station_Remastered.Utils.Requests.UserServices
{
    public class ApplyOfferListAndGetTransactionHistoryRequest
    {
        public List<string> OfferIds { get; set; }
        public int HistoryFromTime { get; set; }
        public int HistoryMaxResults { get; set; }
    }
}
