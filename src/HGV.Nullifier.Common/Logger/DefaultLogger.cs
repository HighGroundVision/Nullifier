using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HGV.Nullifier.Logger
{
    public class DefaultLogger : ILogger
    {
        public void Error(Exception ex, int id = 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(ex.Message);
            Console.ResetColor();
        }

        public void Warning(string msg, int id = 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(msg);
            Console.ResetColor();
        }

        public void Info(string msg, int id = 0)
        {
            Console.WriteLine(msg);
        }
    }
}
