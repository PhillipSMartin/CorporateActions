using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using LoggingUtilitiesLib;
using CorporateActionsLib;
using HugoCorporateActionsLib;
using GargoyleTaskLib;
using Gargoyle.Utils.DBAccess;
using Gargoyle.Common;

namespace CorporateActionsImport
{
    public class Importer : IDisposable
    {
        private ILog m_logger;
        private System.Data.SqlClient.SqlConnection m_hugoConnection;
        private System.Data.SqlClient.SqlConnection m_configDbConnection;

        private ICorporateActionsReader m_reader;
        private ICorporateActionsWriter m_writer;
        private CommandLineParameters m_parms;
        private string m_lastErrorMessage;
        private string m_completionMessage;
        private bool m_taskStarted = false;
        private bool m_taskSucceeded = false;

        #region Housekeeping
        public Importer(CommandLineParameters parms)
        {
            if (parms == null)
                throw new ArgumentNullException("parms");

            m_parms = parms;
         }


        #region Initialize components
        private bool Initialize()
        {
            if (!InitializeLog4Net())
                return false;

            if (!GetDatabaseConnections())
                return false;

            if (!SelectReader(m_parms.Reader))
                return false;

            if (!SelectWriter(m_parms.Writer))
                return false;

            if (!InitializeWriter(m_parms.Writer))
                return false;

            return true;
        }
        private bool SelectReader(string reader)
        {
            try
            {
                m_reader = ReaderFactory.GetReader(reader);
                if (m_reader == null)
                {
                    OnInfo(String.Format("Unable to instantiate corporate actions reader {0}", reader));
                    return false;
                }
                else
                {
                    m_reader.OnInfo += new EventHandler<LoggingEventArgs>(CorporateActionsLib_OnInfo);
                    m_reader.OnError += new EventHandler<LoggingEventArgs>(CorporateActionsLib_OnError);
                    m_reader.WaitMs = m_parms.Timeout;
                    return true;
                }
            }
            catch (Exception ex)
            {
                OnError("Unable to instantiate report reader", ex);
                return false;
            }
        }
        private bool SelectWriter(string writer)
        {
            try
            {
                m_writer = WriterFactory.GetWriter(writer);
                if (m_writer == null)
                {
                    OnInfo(String.Format("Unable to instantiate corporate actions writer {0}", writer));
                    return false;
                }
                else
                {
                    m_writer.OnInfo += new EventHandler<LoggingEventArgs>(CorporateActionsLib_OnInfo);
                    m_writer.OnError += new EventHandler<LoggingEventArgs>(CorporateActionsLib_OnError);
                    return true;
                }
            }
            catch (Exception ex)
            {
                OnError("Unable to instantiate report writer", ex);
                return false;
            }
        }

        private bool GetDatabaseConnections()
        {
            DBAccess dbAccess = DBAccess.GetDBAccessOfTheCurrentUser(m_parms.ProgramName);
            m_hugoConnection = dbAccess.GetConnection("Hugo");
            if (m_hugoConnection == null)
            {
                OnInfo("Unable to connect to Hugo");
                return false;
            }
            m_configDbConnection = dbAccess.GetConnection("ConfigDB");
            if (m_configDbConnection == null)
            {
                OnInfo("Unable to connect to ConfigDb");
                return false;
            }
            return true;
        }

        private bool InitializeLog4Net()
        {
            // initialize log4net via app.config
            log4net.Config.XmlConfigurator.Configure();
            m_logger = LogManager.GetLogger(typeof(Importer));

            LoggingUtilities.OnInfo += LoggingUtilities_OnInfo;
            LoggingUtilities.OnError += LoggingUtilities_OnError;
            TaskUtilities.OnInfo += LoggingUtilities_OnInfo;
            TaskUtilities.OnError += LoggingUtilities_OnError;

            return (m_logger != null);
        }
        private bool InitializeWriter(string writer)
        {
            try
            {
                switch (writer)
                {
                    case "Log":
                        if (m_writer.Init())
                            return true;
                        break;
                    case "Hugo":
                        if (m_writer.Init(m_hugoConnection, m_parms.Timeout))
                            return true;
                        break;
                    default:
                        break;
                }
                OnInfo(String.Format("Unable to initialzie corporate actions writer {0}", writer));
            }
            catch (Exception ex)
            {
                OnError("Unable to initialize corporate actions writer", ex);
            }
            return false;
        }

        private object[] GetParameterList()
        {
            switch (m_parms.Reader)
            {
                case "RealTick":
                case "Realtick":
                    return new object[] { m_configDbConnection };

                case "TWS":
                    string ipAddress = null;
                      int? port = null;
                      int? quoteType = null;

                  if (!String.IsNullOrEmpty(m_parms.IPAddress))
                        ipAddress = m_parms.IPAddress;
                    if (m_parms.Port >= 0)
                        port = m_parms.Port;
                    if (m_parms.DelayedQuotes)
                        quoteType = TWSLib.TWSUtilities.DELAYED;

                    return new object[] { ipAddress, port, quoteType };

                default:
                    return new object[] { };
            }
        }

        private bool Login()
        {
            try
            {
                while (true)
                {
                    if (m_reader.Init(GetParameterList()))
                    {
                        if (m_reader.Connect())
                            return true;

                        // if unable to start, retry as many times as specified in command parameters
                        m_reader.Dispose();
                        if (!Retry("Unable to log in"))
                            return false;
                        if (!SelectReader(m_parms.Reader))
                            return false;
                    }
                    else
                    {
                        OnError("Unable to initialize corporate actions reader");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                OnError("Unable to log in", ex);
            }
            return false;
        }

        private bool Retry(string msg)
        {

            if (m_parms.RetryCount-- != 0)
            {
               OnError(msg + String.Format(" - retrying in {0} seconds", m_parms.RetryThrottle / 1000));

               if (m_taskStarted)
                   m_taskStarted = !EndTask(false);
                System.Threading.Thread.Sleep(m_parms.RetryThrottle);

                m_taskStarted = (0 == StartTask());
                return true;
            }
            else
            {
                OnError(msg + " - exceeded retry attempts");
                return false;
            }
        }
        #endregion

        #region Logging
        // wired to Utilities and provider
        private void LoggingUtilities_OnError(object sender, LoggingEventArgs e)
        {
            if (m_logger != null)
            {
                m_logger.Error(e.Message, e.Exception);
            }
        }
        private void LoggingUtilities_OnInfo(object sender, LoggingEventArgs e)
        {
            if (m_logger != null)
            {
                m_logger.Info(e.Message);
            }
        }
        // events from reader
        private void CorporateActionsLib_OnInfo(object sender, LoggingEventArgs eventArgs)
        {
            OnInfo(eventArgs.Message);
        }

        private void CorporateActionsLib_OnError(object sender, LoggingEventArgs eventArgs)
        {
            OnError(eventArgs.Message, eventArgs.Exception);
        }
        // called by Importer
        private void OnError(string message, Exception ex)
        {
            m_lastErrorMessage = ex.Message;
            if (m_logger != null)
            {
                m_logger.Error(message, ex);
            }
        }
        private void OnError(string message)
        {
            m_lastErrorMessage = message;
            if (m_logger != null)
            {
                m_logger.Info(message);
            }
        }
        private void OnInfo(string message)
        {
            if (m_logger != null)
            {
                m_logger.Info(message);
            }
        }
        #endregion

        #region IDisposable Members
        public void Dispose()
        {
             Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected void Dispose(bool disposing)
        {

            if (disposing)
            {
                if (m_reader != null)
                {
                    m_reader.Dispose();
                    m_reader = null;
                }
                if (m_writer != null)
                {
                    m_writer.Dispose();
                    m_writer = null;
                }
                if (m_hugoConnection != null)
                {
                    m_hugoConnection.Dispose();
                    m_hugoConnection = null;
                }
                if (m_configDbConnection != null)
                {
                    m_configDbConnection.Dispose();
                    m_configDbConnection = null;
                }
            }
        }

        #endregion

        #endregion

       #region Public Properties and Methods
   
        public bool Run()
        {
            try
            {
                if (Initialize())
                {
                    int rc = StartTask();
                    if (rc == 1)
                    {
                        OnInfo("Task not started because it is a holiday");
                    }
                    else
                    {
                        // if task wasn't started (which probably means taskname was not in the table), so be it - no need to abort
                        m_taskStarted = (rc == 0);

                        if (Login())
                        {
                            m_taskSucceeded = PerformTask();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                OnError("Exception thrown by Run method", ex);
            }
            finally
            {
                if (m_taskStarted)
                {
                    EndTask(m_taskSucceeded);
                }
            }
            return m_taskSucceeded;
        }
        #endregion

        #region Private Methods
       private bool PerformTask()
       {
           bool taskSucceeded = false;
           switch (m_parms.Action)
           {
               case CorporateActionsImportActions.Import:
                   taskSucceeded = DoImport();
                   break;
               default:
                   OnError(String.Format("Action {0} not supported", m_parms.Action));
                   break;
           }
           if (m_reader.HadError)
           {
               taskSucceeded = false;
               m_lastErrorMessage = m_reader.LastErrorMessage;
           }
           else if (HugoCorporateActionsUtilities.HadError)
           {
               taskSucceeded = false;
               m_lastErrorMessage = HugoCorporateActionsUtilities.LastErrorMessage;
           }

           return taskSucceeded;
       }
       
        private bool DoImport()
        {
            try
            {
                int countForThisSymbol = 0;
                int totalCount = 0;
                foreach (string underlyingSymbol in new List<string>(GetUnderlyings()))
                {
                    string xml;
                    countForThisSymbol = GetCorporateActions(underlyingSymbol, out xml);

                    if (countForThisSymbol > 0)
                    {
                        totalCount += m_writer.InsertCorporateActions(m_parms.Reader.ToString(), xml, m_parms.GetImportDate());
                    }
                }

                m_completionMessage = String.Format("Imported {0} corporate actions", totalCount);
                OnInfo(m_completionMessage);
                return true;
            }
            catch (Exception ex)
            {
                OnError("Import failed", ex);
                return false;
            }
        }

        private string[] GetUnderlyings()
        {
            if (String.IsNullOrEmpty(m_parms.UnderlyingSymbol))
            {
                return m_writer.GetAllStocks(m_parms.GetImportDate());
            }
            else
            {
                return new string[] { m_parms.UnderlyingSymbol };
            }
        }

        private int GetCorporateActions(string underlyingSymbol, out string xml)
        {
            xml = null;
            DateTime? startMinusOneDate = null;

            int numdays = m_parms.NumberOfDays; // number of days of corporate actions to import. -1 means 'import all'
            if (numdays >= 0)
                startMinusOneDate = m_parms.GetImportDate().Subtract(new TimeSpan(numdays, 0, 0, 0)); 

            return m_reader.GetCorporateActions(underlyingSymbol, startMinusOneDate, out xml);
        }

        private int StartTask()
        {
            try
            {
                using (TaskUtilities taskUtilities = new TaskUtilities(m_hugoConnection, m_parms.Timeout))
                {
                    return taskUtilities.TaskStarted(m_parms.TaskName, null);
                }
            }
            catch (Exception ex)
            {
                OnError("Unable to start task", ex);
                return 16;
            }
        }

        private bool EndTask(bool succeeded)
        {
            try
            {
                using (TaskUtilities taskUtilities = new TaskUtilities(m_hugoConnection, m_parms.Timeout))
                {
                    if (succeeded)
                        return (0 != taskUtilities.TaskCompleted(m_parms.TaskName, m_completionMessage));
                    else
                        return (0 != taskUtilities.TaskFailed(m_parms.TaskName, m_lastErrorMessage));
                }
            }
            catch (Exception ex)
            {
                OnError("Unable to stop task", ex);
                return false;
            }
        }
        #endregion
    }
}
