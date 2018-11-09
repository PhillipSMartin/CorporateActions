using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gargoyle.Common;
using HugoCorporateActionsLib;

namespace CorporateActionsLib
{
    class HugoCorporateActionsWriter : HugoCorporateActionsUtilities, ICorporateActionsWriter
    {
        public HugoCorporateActionsWriter()
        {
            HugoCorporateActionsUtilities.OnInfo += HugoCorporateActions_OnInfo;
            HugoCorporateActionsUtilities.OnError += HugoCorporateActions_OnError;
        }

        #region Events
        private event EventHandler<LoggingEventArgs> m_infoEventHandler;
        private event EventHandler<LoggingEventArgs> m_errorEventHandler;

        public new event EventHandler<LoggingEventArgs> OnError
        {
            add { m_errorEventHandler += value; }
            remove { m_errorEventHandler -= value; }
        }
        public new event EventHandler<LoggingEventArgs> OnInfo
        {
            add { m_infoEventHandler += value; }
            remove { m_infoEventHandler -= value; }
        }
 
        void HugoCorporateActions_OnInfo(object sender, EventArgs e)
        {
            if (m_infoEventHandler != null)
            {
                HugoCorporateActionsLoggingEventArgs args = (HugoCorporateActionsLoggingEventArgs)e;
                m_infoEventHandler(null, new LoggingEventArgs(args.Source, args.Message));
            }
        }
        void HugoCorporateActions_OnError(object sender, EventArgs e)
        {
            if (m_errorEventHandler != null)
            {
                HugoCorporateActionsLoggingEventArgs args = (HugoCorporateActionsLoggingEventArgs)e;
                m_errorEventHandler(null, new LoggingEventArgs(args.Source, args.Message, args.Exception));
            }
        }
        #endregion

        #region Overrides
        public bool Init(params object[] paramList)
        {
            try
            {
                if (paramList.Length == 1)
                {
                    return base.Init((System.Data.SqlClient.SqlConnection)paramList[0]);
                }
                else if (paramList.Length == 2)
                {
                    return base.Init((System.Data.SqlClient.SqlConnection)paramList[0], (int)paramList[1]);
                }
                else
                {
                    if (m_infoEventHandler != null)
                        m_infoEventHandler(null, new LoggingEventArgs("CorporateActionsLib", "Must pass a connection to the Hugo database and (optionally) a timeout value in milleseconds"));
                }
            }
            catch (Exception ex)
            {
                if (m_errorEventHandler != null)
                    m_errorEventHandler(null, new LoggingEventArgs("CorporateActionsLib", "Unable to initialize HugoCorporateActionsUtilities", ex));
            }
            return false;
        }

        public new bool HadError { get { return HugoCorporateActionsUtilities.HadError; } }
        public new string LastErrorMessage { get { return HugoCorporateActionsUtilities.LastErrorMessage; } }
        #endregion
   }
}
