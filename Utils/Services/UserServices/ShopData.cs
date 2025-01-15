using static System.Collections.Specialized.BitVector32;

namespace Halal_Station_Remastered.Utils.Services.UserServices
{
    public class ShopData
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public int Race { get; set; }
        public List<Section> Sections { get; set; }
    }
}