using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gargoyle.Utils.DBAccess;
using System.Data.SqlClient;
using LoggingUtilitiesLib;

namespace HugoCorporateActionsLib
{
    public class HugoCorporateActionsUtilities
    {
        private SqlConnection m_hugoConnection;
        private int m_timeOut;

        #region Event handlers
        private static event EventHandler s_infoEventHandler;
        private static event EventHandler s_errorEventHandler;

        // event fired when an exception occurs
        public static event EventHandler OnError
        {
            add { s_errorEventHandler += value; }
            remove { s_errorEventHandler -= value; }
        }
        // event fired for logging
        public static event EventHandler OnInfo
        {
            add { s_infoEventHandler += value; }
            remove { s_infoEventHandler -= value; }
        }
        #endregion

        #region Public Properties
        public bool IsInitialized { get; private set; }
        public static bool HadError { get; private set; }
        public static string LastErrorMessage { get; private set; }

        #endregion

        #region Public methods
        public bool Init(SqlConnection sqlConnection, int timeOut = 10000)
        {
            if (!IsInitialized)
            {
                try
                {
                    if (sqlConnection == null)
                       throw new ArgumentNullException("sqlConnection");
                    m_hugoConnection = sqlConnection;
                    m_timeOut = timeOut;

                    Info("HugoCorporateActionsUtilities SQL connection = " + sqlConnection.ConnectionString);

                    IsInitialized = true;
                    Info("HugoCorporateActionsUtilities initialized");
                }
                catch (Exception ex)
                {
                    Error("Unable to initialize HugoCorporateActionsUtilities", ex);
                }
            }
            return IsInitialized;
        }
 
        public void Dispose()
        {
            IsInitialized = false;
            if (m_hugoConnection != null)
            {
                m_hugoConnection.Dispose();
                m_hugoConnection = null;
                Info("Hugo connection disposed");
            }
        }

        public int InsertCorporateActions(string provider, string xml, DateTime? importDate)
        {
            return InsertCorporateActions(provider, xml, importDate, false);
        }

        // returns 0 on success
        public int AddSpinoff(string symbol, DateTime eventDate, string xml="<Deliverables></Deliverables>")
        {
            ClearErrorState();
            int rc = 8;

            try
            {
                if ((!IsInitialized) || (m_hugoConnection == null))
                    throw new Exception("HugoCorporateActionsLib not initialized");

                using (HugoDataSetTableAdapters.QueriesTableAdapter queryAdapter = new HugoDataSetTableAdapters.QueriesTableAdapter())
                {
                    queryAdapter.Connection = m_hugoConnection;
                    queryAdapter.SetAllCommandTimeouts(m_timeOut);
                    queryAdapter.p_add_spinoff(null, 0, eventDate, null, null, null, null, symbol, null, null, null, xml);

                    queryAdapter.LogSqlCommand("CorporateActions.p_add_spinoff");
                    rc = queryAdapter.GetReturnCode("CorporateActions.p_add_spinoff");
                }

            }
            catch (Exception ex)
            {
                 Error("Error inserting spinoffs", ex);
            }

            Info(String.Format("Inserted spinoff"));
            return rc;
        }
        public int InsertCorporateActions(string provider, string xml, DateTime? importDate, bool restart)
        {
            int? numberRecordsInserted = 0;
            ClearErrorState();

            try
            {
                if ((!IsInitialized) || (m_hugoConnection == null))
                    throw new Exception("HugoCorporateActionsLib not initialized");

                 using (HugoDataSetTableAdapters.QueriesTableAdapter queryAdapter = new HugoDataSetTableAdapters.QueriesTableAdapter())
                {
                   queryAdapter.Connection = m_hugoConnection;
                   queryAdapter.SetAllCommandTimeouts(m_timeOut);
                   queryAdapter.p_insert_CorporateActions(provider, xml, importDate, ref numberRecordsInserted, restart, false);

                   queryAdapter.LogSqlCommand("CorporateActions.p_insert_CorporateActions");
                 }

              }
            catch (Exception ex)
            {
                numberRecordsInserted = 0;
                Error("Error inserting corporate actions", ex);
            }

            if (numberRecordsInserted.Value > 0)
                Info("Break");
            Info(String.Format("Inserted {0} corporate actions", numberRecordsInserted));
            return numberRecordsInserted.Value;
        }

        public string[] GetAllStocks(DateTime? importDate = null)
        {
            List<string> stockList = new List<string>();
            ClearErrorState();

            try
            {
                if ((!IsInitialized) || (m_hugoConnection == null))
                    throw new Exception("HugoCorporateActionsLib not initialized");

                HugoDataSet.AllStocksDataTable  table;
                using (HugoDataSetTableAdapters.AllStocksTableAdapter tableAdapter = new HugoDataSetTableAdapters.AllStocksTableAdapter())
                {
                    tableAdapter.Connection = m_hugoConnection;
                    table = tableAdapter.GetData(importDate);

                    tableAdapter.LogSqlCommand();
                }

                if (table != null)
                {
                    foreach (HugoDataSet.AllStocksRow row in table.Rows)
                    {
                        stockList.Add(row.StockSymbol);
                    }
                }

            }
            catch (Exception ex)
            {
                Error("Error getting all stocks", ex);
            }

            Info(String.Format("Retrieved {0} stocks", stockList.Count));
            return stockList.ToArray();
        }
        #endregion

        #region Private Methods
        private static void Info(string msg)
        {
            try
            {
                if (s_infoEventHandler != null)
                {
                    s_infoEventHandler(null, new HugoCorporateActionsLoggingEventArgs("HugoCorporateActionsLib", msg));
                }
            }
            catch
            {
            }
         }

        private static void Error(string msg, Exception e)
        {
            try
            {
                HadError = true;
                LastErrorMessage = e.Message;
                if (s_errorEventHandler != null)
                    s_errorEventHandler(null, new HugoCorporateActionsLoggingEventArgs("HugoCorporateActionsLib", msg, e));
            }
            catch
            {
            }
        }

        private static void ClearErrorState()
        {
            HadError = false;
            LastErrorMessage = null;
        }
        #endregion
    }
}
