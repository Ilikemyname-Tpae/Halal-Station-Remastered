namespace Halal_Station_Remastered.Utils.Services.UserServices
{
    public class Shelf
    {
        public string Name { get; set; }
        public bool IsHot { get; set; }
        public bool IsSale { get; set; }
        public List<string> Items { get; set; }
    }
}