using HGV.Nullifier;
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
        static void Main(string[] args)
        {
            var apiKey = System.Configuration.ConfigurationManager.AppSettings["DotaApiKey"].ToString();
            var pastTarget = long.Parse(System.Configuration.ConfigurationManager.AppSettings["PastTarget"].ToString());
            var defaultLogger = new DefaultLogger();
            var cancellationSource = new CancellationTokenSource();
            Console.CancelKeyPress += (object sender, ConsoleCancelEventArgs e) => { cancellationSource.Cancel(); };

            StatCollectionHandler.Run(defaultLogger, apiKey, pastTarget, cancellationSource.Token);
        }
    }
}
