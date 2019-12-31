using System;
using System.Threading;
using System.Threading.Tasks;
using HGV.Nullifier;
using HGV.Nullifier.Logger;

namespace HGV.Nullifier.Tools.Export
{
    class Program
    {
        static void Main(string[] args)
        {
            var logger = new DefaultLogger();

            var settings = System.Configuration.ConfigurationManager.AppSettings;
            var apiKey = settings["DotaApiKey"].ToString();
            var outputDirectory = settings["OutputDirectory"].ToString() ?? Environment.CurrentDirectory;
            
            var handler = new StatExportHandler(logger, apiKey, outputDirectory);
            handler.Run();
        }
    }
}
