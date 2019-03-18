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

        public static V GetValueOrDefault<T, V>(this IDictionary<T, V> map, T key)
        {
            V value;
            if (map.TryGetValue(key, out value))
            {
                return value;
            }
            else
            {
                return default(V);
            }
        }

        public static List<List<T>> Split<T>(this List<T> items, int sliceSize = 100)
        {
            List<List<T>> list = new List<List<T>>();

            for (int i = 0; i < items.Count; i += sliceSize)
                list.Add(items.GetRange(i, Math.Min(sliceSize, items.Count - i)));

            return list;
        }
    }
}
