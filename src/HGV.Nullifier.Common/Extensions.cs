using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HGV.Nullifier
{
    public static class Extensions
    {
        public static (double, double, double, double) Deviation<T>(this IEnumerable<T> list, Func<T, double> values)
        {
            var mean = 0.0;
            var sum = 0.0;
            var stdDev = 0.0;
            var max = 0.0;
            var min = 0.0;
            var n = 0;
            foreach (var value in list.Select(values))
            {
                n++;
                var delta = value - mean;
                mean += delta / n;
                sum += delta * (value - mean);
                if (value > max) max = value;
                if( value < min) min = value;
            }

            if (1 < n) stdDev = Math.Sqrt(sum / (n - 1));

            return (stdDev, mean, max, min);
        }
    }
}
