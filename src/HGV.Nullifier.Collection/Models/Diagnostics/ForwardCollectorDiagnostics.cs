using System;
using System.Collections.Generic;
using System.Text;

namespace HGV.Nullifier.Collection.Models.Diagnostics
{
    public class ForwardCollectorDiagnostics
    {
        public long StartTime { get; set; }
        public long Duration { get; set; }
        public TimeSpan Delta => DateTimeOffset.UtcNow - DateTimeOffset.FromUnixTimeSeconds(StartTime + Duration);
        public long Current { get; set; }
        public long MatchesProcessed { get; set; }
        public long MatchesCollected { get; set; }
        public TimeSpan Sleeping { get; set; }
    }
}
