using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gargoyle.Common;

namespace CorporateActionsLib
{
    class LoggingCorporateActionsWriter : ICorporateActionsWriter
    {
        public bool Init(params object[] paramList) { IsInitialized = true; return true; }
 
        public bool IsInitialized { get; private set; }
        public bool HadError { get; private set; }
        public string LastErrorMessage { get; private set; }

        #region Events
        private event EventHandler<LoggingEventArgs> m_infoEventHandler;
        private event EventHandler<LoggingEventArgs> m_errorEventHandler;

        public event EventHandler<LoggingEventArgs> OnError
        {
            add { m_errorEventHandler += value; }
            remove { m_errorEventHandler -= value; }
        }
        public event EventHandler<LoggingEventArgs> OnInfo
        {
            add { m_infoEventHandler += value; }
            remove { m_infoEventHandler -= value; }
        }
        #endregion

        // returns list of underlyings to get corporateactions on
        public string[] GetAllStocks(DateTime? importDate)
        {
            return new string[]
            {
                "EEM",
                "EFA",
                "SPY",
                "SPX",
                "IWM",
                "NDX",
                "RUT"
            };
        }

        // returns number of corporate actions inserted
        public int InsertCorporateActions(string provider, string xml, DateTime? importDate)
        {
            if (IsInitialized)
            {
                try
                {
                    if (m_infoEventHandler != null)
                    {
                        m_infoEventHandler(null, new LoggingEventArgs("LoggingCorporateActionsWriter", xml));
                        return 1;
                    }
                }
                catch (Exception ex)
                {
                    if (m_errorEventHandler != null)
                    {
                        m_errorEventHandler(null, new LoggingEventArgs("LoggingCorporateActionsWriter", "Error in logger", ex));
                    }
                }
            }
            return 0;
        }

        public void Dispose() { }
    }
}
