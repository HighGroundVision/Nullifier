using HGV.Nullifier;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HGV.Nullifer.Service
{
    public partial class MainService : ServiceBase
    {
        protected Thread mainThread;
        CancellationToken cancelToken;
        CancellationTokenSource tokenSource;

        public MainService()
        {
            InitializeComponent();

            // Event Log
            this.MainEvenLog = new System.Diagnostics.EventLog();
            if (System.Diagnostics.EventLog.SourceExists("HGV.Nullifer"))
            {
                this.MainEvenLog.Source = "HGV.Nullifer";
                this.MainEvenLog.Log = "Application";
            }
            else
            {
                System.Diagnostics.EventLog.CreateEventSource("HGV.Nullifer", "Application");
            }

            //  Cancellation from OnStop()
            this.tokenSource = new CancellationTokenSource();
            this.cancelToken = tokenSource.Token;
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
                var handler = new StatCollectionHandler();
                var tasks = new List<Task> { handler.Log(this.EventLog), handler.Collect(), handler.Count(), handler.Process() }.ToArray();
                Task.WaitAny(tasks, this.cancelToken);
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
