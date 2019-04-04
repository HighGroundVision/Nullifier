using HGV.Nullifier.Logger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HGV.Nullifer.Service
{
    public class EventLogger : ILogger
    {
        private EventLog events;

        public EventLogger(EventLog e)
        {
            this.events = e;
        }

        public void Error(Exception ex)
        {
            this.events.WriteEntry(ex.Message, EventLogEntryType.Error);
        }

        public void Warning(string msg)
        {
            this.events.WriteEntry(msg);
        }

        public void Info(string msg)
        {
            this.events.WriteEntry(msg);
        }
    }
}
