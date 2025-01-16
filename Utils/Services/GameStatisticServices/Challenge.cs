using System.Diagnostics.Metrics;

namespace Halal_Station_Remastered.Utils.Services.GameStatisticServices
{
    public class Challenge
    {
        public string ChallengeId { get; set; }
        public int Progress { get; set; }
        public long FinishedAtUnixMilliseconds { get; set; }
        public long StartDateUnixMilliseconds { get; set; }
        public long EndDateUnixMilliseconds { get; set; }
        public List<Counter> Counters { get; set; }
        public List<Reward> Rewards { get; set; }
    }
}