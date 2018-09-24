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
            var index = Task.WaitAny(handler.Collect(), handler.Count(), handler.Process(), handler.Report());

            var collection = new Dictionary<int, string>() {
                { 0, "Error in Collect()" },
                { 1, "Error in Count()" },
                { 2, "Error in Process()" },
                { 3,"Error in Report()" },
            };
            Console.WriteLine(collection[index]);
            Console.ReadKey();
        }
    }
}
