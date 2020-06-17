using System;
using System.Collections.Generic;
using System.Text;

namespace HGV.Nullifier.Collection.Models
{
    public class DiagnosticData
    {
        public long StartTime { get; set; }
        public long Duration { get; set; }
        public TimeSpan Delta => DateTimeOffset.UtcNow - DateTimeOffset.FromUnixTimeSeconds(StartTime + Duration);
        public long Current { get; set; }
        public long MatchesProcessed { get; set; }
        public long MatchesCollected { get; set; }
        public TimeSpan InError { get; set; }
    }
}
