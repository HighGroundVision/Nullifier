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
        private List<string> messages;

        public EventLogger(EventLog e)
        {
            this.messages = new List<string>();
            this.events = e;
        }

        public void Error(Exception ex)
        {
            this.Flush(true);
            this.events.WriteEntry(ex.Message, EventLogEntryType.Error);
        }

        public void Warning(string msg)
        {
            this.messages.Add(msg);
            this.Flush(false);
        }

        public void Info(string msg)
        {
            this.messages.Add(msg);
            this.Flush(false);
        }
     
        private void Flush(bool now = false)
        {
            if(this.messages.Count >= 10 || now)
            {
                var entry = string.Join(Environment.NewLine, this.messages.ToArray());
                this.events.WriteEntry(entry);
                this.messages.Clear();
            }
        }
    }
}
