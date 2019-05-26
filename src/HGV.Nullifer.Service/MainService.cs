using HGV.Nullifier;
using HGV.Nullifier.Logger;
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

            // signal the event to shutdown
            this.tokenSource.Cancel();

            // wait for the thread to stop giving it 10 seconds
            mainThread.Join(TimeSpan.FromSeconds(10));

            MainEvenLog.WriteEntry($"${this.ServiceName} has stopped");
        }

        protected void ServiceMain()
        {
            try
            {
                var apiKey = System.Configuration.ConfigurationManager.AppSettings["DotaApiKey"].ToString();
                var pastTarget = long.Parse(System.Configuration.ConfigurationManager.AppSettings["PastTarget"].ToString());
                var eventLogger = new EventLogger(this.MainEvenLog);
                StatCollectionHandler.Run(eventLogger, apiKey, pastTarget, this.tokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                MainEvenLog.WriteEntry($"Handler is canceled");
            }
            catch(Exception ex)
            {
                MainEvenLog.WriteEntry(ex.Message, EventLogEntryType.Error, 100);
            }
        }
    }
}
