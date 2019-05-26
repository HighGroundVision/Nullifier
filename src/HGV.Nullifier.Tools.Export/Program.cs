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
            var defaultLogger = new DefaultLogger();

            var settings = System.Configuration.ConfigurationManager.AppSettings;
            var apiKey = settings["DotaApiKey"].ToString();
            var outputDirectory = settings["OutputDirectory"].ToString() ?? Environment.CurrentDirectory;

            StatExportHandler.Run(defaultLogger, apiKey, outputDirectory);

            Console.WriteLine("Press any key to continue");
            Console.Read();
        }
    }
}
