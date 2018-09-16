using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HGV.Nullifier.Tools.Collection
{
    class Program
    {
        static void Main(string[] args)
        {
            var handler = new StatCollectionHandler();
            var index = Task.WaitAny(handler.Report(), handler.Collect(), handler.Count(), handler.Process());

            var collection = new Dictionary<int, string>() {
                { 0, "Error in Report()" },
                { 1, "Error in Collect()" },
                { 2, "Error in Count()" },
                { 3, "Error in Process()" }
            };
            System.Diagnostics.Debug.WriteLine(collection[index]);

            Console.ReadKey();
        }
    }
}
