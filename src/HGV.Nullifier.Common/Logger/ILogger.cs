using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HGV.Nullifier.Logger
{
    public interface ILogger
    {
        void Info(string msg, int id = 0);
        void Warning(string msg, int id = 0);
        void Error(Exception ex, int id = 0);
    }
}
