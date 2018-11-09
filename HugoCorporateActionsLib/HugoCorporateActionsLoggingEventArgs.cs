using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HugoCorporateActionsLib
{
    public class HugoCorporateActionsLoggingEventArgs : EventArgs
    {
        public HugoCorporateActionsLoggingEventArgs(string source, string message, Exception ex = null)
        {
            Source = source;
            Message = message;
            Exception = Exception;
        }

        public string Message { get; protected set; }
        public string Source { get; protected set; }
        public Exception Exception { get; protected set; }
   }
}
