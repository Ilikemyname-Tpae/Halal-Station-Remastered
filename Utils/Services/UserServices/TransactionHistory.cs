namespace Halal_Station_Remastered.Utils.Services.UserServices
{
    public class TransactionHistory
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string StateName { get; set; }
        public int StateType { get; set; }
        public int OwnType { get; set; }
        public int OperationType { get; set; }
        public int InitialValue { get; set; }
        public int ResultingValue { get; set; }
        public int DeltaValue { get; set; }
        public int DescId { get; set; }
        public string SessionId { get; set; }
        public string ReferenceId { get; set; }
        public string OfferId { get; set; }
        public long TimeStamp { get; set; }
        public string ExtendedInfoKey { get; set; }
        public string ExtendedInfoValue { get; set; }
    }
}
