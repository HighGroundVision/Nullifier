using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HGV.Nullifier.Logger
{
    public class DefaultLogger : ILogger
    {
        public void Error(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        public void Info(string msg)
        {
            Console.WriteLine(msg);
        }
    }
}
