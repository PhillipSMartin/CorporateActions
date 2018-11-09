using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gargoyle.Common;
using TWSLib;

namespace CorporateActionsLib
{
    class TWSCorporateActionsReader : TWSUtilities, ICorporateActionsReader
    {
        public TWSCorporateActionsReader()
        {
            base.OnInfo += TWSCorporateActions_OnInfo;
            base.OnError += TWSCorporateActions_OnError;
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

        void TWSCorporateActions_OnInfo(object sender, EventArgs e)
        {
            if (m_infoEventHandler != null)
            {
                LoggingEventArgs args = (LoggingEventArgs)e;
                m_infoEventHandler(null, new LoggingEventArgs(args.Source, args.Message));
            }
        }
        void TWSCorporateActions_OnError(object sender, EventArgs e)
        {
            if (m_errorEventHandler != null)
            {
                LoggingEventArgs args = (LoggingEventArgs)e;
                m_errorEventHandler(null, new LoggingEventArgs(args.Source, args.Message, args.Exception));
            }
        }
        #endregion

        #region Overrides
        // all parameters are optional
        // 1) IPAddress - defaults to local host
        // 2) Port - defaults to 7496
        // 3) quoteType - use TWSLib.TWSUtilities.DELAYED if getting delay quotes, otherwise leave null
        public bool Init(params object[] paramList)
        {
            bool success = false;
            try
            {
                if (paramList.Length == 0)
                {
                    success = base.Init(TWSUtilities.CORPORATE_ACTIONS_READER);
                }
                else if (paramList.Length == 1)
                {
                    success = base.Init(TWSUtilities.CORPORATE_ACTIONS_READER, (string)paramList[0]);
                }
                else if (paramList.Length == 2)
                {
                    success = base.Init(TWSUtilities.CORPORATE_ACTIONS_READER, (string)paramList[0], (int?)paramList[1]);
                }
                else if (paramList.Length == 3)
                {
                    success = base.Init(TWSUtilities.CORPORATE_ACTIONS_READER, (string)paramList[0], (int?)paramList[1], (int?)paramList[2]);
                }
                else
                {
                    if (m_infoEventHandler != null)
                        m_infoEventHandler(null, new LoggingEventArgs("CorporateActionsLib", "Too many parameters specified on Init - parameters are IPAddress, port, and quoteType (all optional)"));
                }

                if (success)
                {
                    success = Connect();
                }

                if (success)
                {
                    success = StartCorporateActionsReader();
                }
            }
            catch (Exception ex)
            {
                if (m_errorEventHandler != null)
                    m_errorEventHandler(null, new LoggingEventArgs("CorporateActionsLib", "Unable to initialize TWSUtilities", ex));
                success = false;
            }
            return success;
        }
        #endregion
    }
}
