using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gargoyle.Common;

namespace CorporateActionsLib
{
    public interface ICorporateActionsReader : IDisposable
    {
        bool Init(params object[] paramList);
        bool Connect();

        // returns number of corporate actions read
        int GetCorporateActions(string symbol, DateTime? startMinusOneDate, out string xml);

        event EventHandler<LoggingEventArgs> OnError;
        event EventHandler<LoggingEventArgs> OnInfo;
 
        bool IsInitialized { get; }
        bool IsConnected { get; }
        int WaitMs { set; }

        bool HadError { get; }
        string LastErrorMessage { get; }
    }
}
