using LoggingUtilitiesLib;

namespace HugoCorporateActionsLib.HugoDataSetTableAdapters {
    partial class AllStocksTableAdapter
    {
        internal void LogSqlCommand()
        {
            LoggingUtilities.LogSqlCommand("SqlLog", CommandCollection, 0);
        }
   }
    
    partial class QueriesTableAdapter
    {
        private global::System.Data.SqlClient.SqlConnection _connection;

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        internal global::System.Data.SqlClient.SqlConnection Connection
        {
            get
            {
                 return this._connection;
            }
            set
            {
                this._connection = value;
                for (int i = 0; (i < this.CommandCollection.Length); i = (i + 1))
                {
                    if ((this.CommandCollection[i] != null))
                    {
                        ((global::System.Data.SqlClient.SqlCommand)(this.CommandCollection[i])).Connection = value;
                    }
                }
            }
        }
        internal void SetAllCommandTimeouts(int timeOut)
        {
            foreach (System.Data.IDbCommand cmd in CommandCollection)
            {
                cmd.CommandTimeout = timeOut;
            }
        }
        internal int GetReturnCode(string commandText)
        {
            return LoggingUtilities.GetReturnCode(CommandCollection, commandText);
        }
        internal void LogSqlCommand(string commandText)
        {
            LoggingUtilities.LogSqlCommand("SqlLog", CommandCollection, commandText);
        }
    }
}

namespace HugoCorporateActionsLib {
    
    
    public partial class HugoDataSet {
    }
}
