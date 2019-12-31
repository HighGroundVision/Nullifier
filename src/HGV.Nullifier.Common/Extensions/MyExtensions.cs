using HGV.Daedalus.GetMatchDetails;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HGV.Nullifier.Common.Extensions
{
    public static class MyExtensions
    {
        public static TimeSpan GetDuration(this Match match)
        {
            return DateTimeOffset.FromUnixTimeSeconds(match.duration).UtcDateTime.TimeOfDay;
        }

        public static DateTime GetStart(this Match match)
        {
            return DateTimeOffset.FromUnixTimeSeconds(match.start_time).UtcDateTime;
        }

        public static DateTime GetEnd(this Match match)
        {
            return DateTimeOffset.FromUnixTimeSeconds(match.start_time + match.duration).UtcDateTime;
        }

        public static double GetKDA(this Player player)
        {
            return player.kills + (player.assists / 3.0f) - player.deaths;
        }
    }
}
