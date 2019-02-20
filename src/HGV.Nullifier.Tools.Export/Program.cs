using System;
using System.Threading;
using HGV.Nullifier;
using HGV.Nullifier.Logger;

namespace HGV.Nullifier.Tools.Export
{
    class Program
    {
        static void Main(string[] args)
        {
            var apiKey = System.Configuration.ConfigurationManager.AppSettings["DotaApiKey"].ToString();
            var defaultLogger = new DefaultLogger();
            var cancellationSource = new CancellationTokenSource();
            Console.CancelKeyPress += (object sender, ConsoleCancelEventArgs e) => { cancellationSource.Cancel(); };

            StatExportHandler.Run(apiKey, cancellationSource.Token, defaultLogger);
        }
    }
}
