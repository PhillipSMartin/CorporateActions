using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace CorporateActionsImport
{
    class Program
    {

        [STAThread]
        static void Main(string[] args)
        {
            CommandLineParameters parms = new CommandLineParameters();
            Importer importer = null;

            string dirDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Gargoyle Strategic Investments\\CorporateActionsImport";
            string appDataPath = dirDataPath + "\\TraceListener.log";
            DirectoryInfo dInfo = new DirectoryInfo(dirDataPath);
            if (!dInfo.Exists)
                dInfo.Create();

            TextWriterTraceListener trace = new TextWriterTraceListener(new StreamWriter(appDataPath, false));

            try
            {

                if (Gargoyle.Utilities.CommandLine.Utility.ParseCommandLineArguments(args, parms))
                {
                    importer = new Importer(parms);
                    if (importer.Run())
                    {
                        Trace.TraceInformation("CorporateActionsImport completed");
                     }
                    else
                    {
                        Trace.TraceInformation("CorporateActionsImport failed - see error log");
                    }
                }
                else
                {
                    // display usage message
                    string errorMessage = Gargoyle.Utilities.CommandLine.Utility.CommandLineArgumentsUsage(typeof(CommandLineParameters));

                    Trace.TraceError(errorMessage);
                 }
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
            }
            finally
            {
                Trace.Flush();
                if (importer != null)
                {
                    importer.Dispose();
                    importer = null;
                }
            }
        }

    }
}

