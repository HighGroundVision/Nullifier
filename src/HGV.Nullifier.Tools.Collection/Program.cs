using HGV.Nullifier.Common.Collection;
using HGV.Nullifier.Logger;
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
        static async Task Main(string[] args)
        {
            var apiKey = System.Configuration.ConfigurationManager.AppSettings["DotaApiKey"].ToString();
            var defaultLogger = new DefaultLogger();

            var handler = new StatCollectionHandler(defaultLogger, apiKey);
            await handler.Run();
        }
    }
}
