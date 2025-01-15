namespace Halal_Station_Remastered.Utils.Services.UserServices
{
    public class ItemOffer
    {
        public string ItemId { get; set; }
        public List<object> Requirements { get; set; }
        public List<object> Unlocks { get; set; }
        public List<BundleItem> BundleItems { get; set; }
        public int UnlockedLevel { get; set; }
        public List<OfferLine> OfferLine { get; set; }
    }
}