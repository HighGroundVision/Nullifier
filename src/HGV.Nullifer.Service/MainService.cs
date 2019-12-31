using HGV.Nullifier.Common.Collection;
using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;

namespace HGV.Nullifer.Service
{
    public partial class MainService : ServiceBase
    {
        protected Thread mainThread;
        CancellationTokenSource tokenSource;

        public MainService()
        {
            InitializeComponent();

            // Event Log
            this.MainEvenLog = new EventLog();

            if (EventLog.SourceExists("HGV.Nullifer"))
            {
                this.MainEvenLog.Source = "HGV.Nullifer";
                this.MainEvenLog.Log = "Application";
            }
            else
            {
                EventLog.CreateEventSource("HGV.Nullifer", "Application");
            }

            //  Cancellation from OnStop()
            this.tokenSource = new CancellationTokenSource();
        }

        protected override void OnStart(string[] args)
        {
            MainEvenLog.WriteEntry($"${this.ServiceName} is starting");

            // create our threadstart object to wrap our delegate method
            ThreadStart ts = new ThreadStart(this.ServiceMain);

            // create the worker thread
            mainThread = new Thread(ts);

            // go ahead and start the worker thread
            mainThread.Start();
        }

        protected override void OnStop()
        {
            MainEvenLog.WriteEntry($"${this.ServiceName} is stopping");

            mainThread.Abort();

            MainEvenLog.WriteEntry($"${this.ServiceName} has stopped");
        }

        protected void ServiceMain()
        {
            try
            {
                var apiKey = System.Configuration.ConfigurationManager.AppSettings["DotaApiKey"].ToString();
                var eventLogger = new EventLogger(this.MainEvenLog);

                var handler = new StatCollectionHandler(eventLogger, apiKey);
                handler.Run().Wait();
            }
            catch(Exception ex)
            {
                MainEvenLog.WriteEntry(ex.Message, EventLogEntryType.Error, 100);
            }
        }
    }
}
