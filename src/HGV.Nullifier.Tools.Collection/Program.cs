using HGV.Daedalus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HGV.Nullifier.Tools.Collection
{
    class Program
    {
        static void Main(string[] args)
        {
            while(true)
            {
                var handler = new StatCollectionHandler();
                var index = Task.WaitAny(handler.Report(), handler.Collect(), handler.Count(), handler.Process());
                if (index == 0)
                    System.Diagnostics.Debug.WriteLine("Error in Report()");
                else if (index == 1)
                    System.Diagnostics.Debug.WriteLine("Error in Collect()");
                else if (index == 2)
                    System.Diagnostics.Debug.WriteLine("Error in Count()");
                else if (index == 3)
                    System.Diagnostics.Debug.WriteLine("Error in Process()");

                System.Threading.Thread.Sleep(TimeSpan.FromMinutes(5));
            }
        }
    }
}
