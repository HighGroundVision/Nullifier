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
            //var defaultLogger = new DefaultLogger();
            //var cancellationSource = new CancellationTokenSource();
            //Console.CancelKeyPress += (object sender, ConsoleCancelEventArgs e) => { cancellationSource.Cancel(); };
            //StatExportHandler.Run(cancellationSource.Token, defaultLogger);

            var defaultLogger = new DefaultLogger();
            var handler = new StatExportHandler(defaultLogger);
            handler.Initialize();
            handler.ExportDraftPool();
            handler.ExportHeroes();
            handler.ExportAbilities();
            handler.ExportUlimates();
            handler.ExportTaltents();

            // handler.ExportAccounts();
        }
    }
}
