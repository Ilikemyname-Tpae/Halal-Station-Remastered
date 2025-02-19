namespace Halal_Station_Remastered.Utils.Services.UserServices
{
    public class Transaction
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string OfferId { get; set; }
        public int InitialValue { get; set; }
        public int ResultingValue { get; set; }
        public int DeltaValue { get; set; }
        public int OperationType { get; set; }
        public string SessionId { get; set; }
        public string ReferenceId { get; set; }
        public long TimeStamp { get; set; }
    }
}
