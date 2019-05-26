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

        public void Error(Exception ex, int id = 0)
        {
            this.events.WriteEntry(ex.Message, EventLogEntryType.Error, id);
        }

        public void Warning(string msg, int id = 0)
        {
            this.events.WriteEntry(msg, EventLogEntryType.Warning, id);
        }

        public void Info(string msg, int id = 0)
        {
            this.events.WriteEntry(msg, EventLogEntryType.Information, id);
        }
    }
}
