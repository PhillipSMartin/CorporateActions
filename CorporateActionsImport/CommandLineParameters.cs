using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gargoyle.Utilities.CommandLine;

namespace CorporateActionsImport
{
    public class CommandLineParameters
    {
        [CommandLineArgumentAttribute(CommandLineArgumentType.AtMostOnce, ShortName = "r", Description = "Source of corporate actions - Currently RealTick or TWS")]
        public string Reader = "RealTick";

        [CommandLineArgumentAttribute(CommandLineArgumentType.AtMostOnce, ShortName = "w", Description = "Repository of reports - currently only Hugo and Log supported")]
        public string Writer = "Hugo";

        [CommandLineArgumentAttribute(CommandLineArgumentType.AtMostOnce, ShortName = "pname", Description = "Name of program to specify to DBAccess")]
        public string ProgramName = "CorporateActionsImport";

        [CommandLineArgumentAttribute(CommandLineArgumentType.AtMostOnce, ShortName = "a", Description = "Action for this execution - defaults to Import")]
        public CorporateActionsImportActions Action = CorporateActionsImportActions.Import;

        [CommandLineArgumentAttribute(CommandLineArgumentType.AtMostOnce, ShortName = "tname", Description = "Task name for reporting completion")]
        public string TaskName = "CorporateActionsUpdate";

        [CommandLineArgumentAttribute(CommandLineArgumentType.AtMostOnce, ShortName = "date", Description = "Import date - defaults to today")]
        public string ImportDate;

        [CommandLineArgumentAttribute(CommandLineArgumentType.AtMostOnce, ShortName = "und", Description = "UnderlyingSymbol - if NULL, all market-making symbols")]
        public string UnderlyingSymbol;

        [CommandLineArgumentAttribute(CommandLineArgumentType.AtMostOnce, ShortName = "numdays", Description = "Number of days to import corporate actions (default is 7) - if -1 import all")]
        public int NumberOfDays = 7;

        [CommandLineArgumentAttribute(CommandLineArgumentType.AtMostOnce, Description = "Time in milleseconds before events time out")]
        public int Timeout = 10000;

        [CommandLineArgumentAttribute(CommandLineArgumentType.AtMostOnce, ShortName = "retry", Description = "Number of times to retry login. -1 means infinite retries.")]
        public int RetryCount = -1;

        [CommandLineArgumentAttribute(CommandLineArgumentType.AtMostOnce, ShortName = "rt", Description = "Time in milliseconds between login retries")]
        public int RetryThrottle = 60000;

        [CommandLineArgumentAttribute(CommandLineArgumentType.AtMostOnce, ShortName = "ip", Description = "IP address for connection to quote reader - if empty, will pass null for default")]
        public string IPAddress = "172.18.0.6";

        [CommandLineArgumentAttribute(CommandLineArgumentType.AtMostOnce, ShortName = "p", Description = "port for connection to quote reader - -1 says to pass null for default")]
        public int Port = -1;

        [CommandLineArgumentAttribute(CommandLineArgumentType.AtMostOnce, ShortName = "dq", Description = "Specify true if quotes are delayed")]
        public bool DelayedQuotes = false;

        public CommandLineParameters()
        {
        }

        public DateTime GetImportDate()
        {
            if (string.IsNullOrEmpty(ImportDate))
                return DateTime.Today;
            else
            {
                DateTime importDate;
                if (DateTime.TryParse(ImportDate, out importDate))
                    return importDate;
                else
                    throw new ApplicationException(
                        string.Format("Cannot convert {0} to a date", ImportDate));
            }
        }
    }
}

