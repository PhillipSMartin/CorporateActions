using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gargoyle.Common;

namespace CorporateActionsLib
{
    public interface ICorporateActionsWriter : IDisposable
    {
        bool Init(params object[] paramList);

        event EventHandler<LoggingEventArgs> OnError;
        event EventHandler<LoggingEventArgs> OnInfo;
 
        bool IsInitialized { get; }

        bool HadError { get; }
        string LastErrorMessage { get; }

        // returns list of underlyings to get corporateactions on
        string[] GetAllStocks(DateTime? importDate);

        // returns number of corporate actions inserted
        int InsertCorporateActions(string provider, string xml, DateTime? importDate);
    }
}
