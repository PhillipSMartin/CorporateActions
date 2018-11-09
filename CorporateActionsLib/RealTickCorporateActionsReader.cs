using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gargoyle.Common;
using RealTickLib;

namespace CorporateActionsLib
{
    class RealTickCorporateActionsReader : RealTickUtilities, ICorporateActionsReader
    {
        public RealTickCorporateActionsReader()
        {
            base.OnInfo += RealTickCorporateActions_OnInfo;
            base.OnError += RealTickCorporateActions_OnError;
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
 
        void RealTickCorporateActions_OnInfo(object sender, EventArgs e)
        {
            if (m_infoEventHandler != null)
            {
                LoggingEventArgs args = (LoggingEventArgs)e;
                m_infoEventHandler(null, new LoggingEventArgs(args.Source, args.Message));
            }
        }
        void RealTickCorporateActions_OnError(object sender, EventArgs e)
        {
            if (m_errorEventHandler != null)
            {
                LoggingEventArgs args = (LoggingEventArgs)e;
                m_errorEventHandler(null, new LoggingEventArgs(args.Source, args.Message, args.Exception));
            }
        }
        #endregion

        #region Overrides
        // there are two ways to initialize RealTickUtilites
        //  1) pass in a SqlConnection to the ConfigDb database - we will look up the proper credentials for the current user
        //  2) pass in three strings: username, password, and domainname
        public bool Init(params object[] paramList)
        {
            try
            {
                if (paramList.Length == 1)
                {
                    return base.Init((System.Data.SqlClient.SqlConnection)paramList[0]);
                }
                else if (paramList.Length == 3)
                {
                    return base.Init((string)paramList[0], (string)paramList[1], (string)paramList[2]);
                }
                else
                {
                    if (m_infoEventHandler != null)
                        m_infoEventHandler(null, new LoggingEventArgs("CorporateActionsLib", "Must pass either a connection to the ConfigDb database or the username, password, and domainname of the realtick id you wish to use"));
                }
            }
            catch (Exception ex)
            {
                if (m_errorEventHandler != null)
                    m_errorEventHandler(null, new LoggingEventArgs("CorporateActionsLib", "Unable to initialize RealTickUtilities", ex));
            }
            return false;
        }
        #endregion
    }
}
