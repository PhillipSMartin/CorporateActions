using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CorporateActionsImport;
using System.Collections.Generic;
using Gargoyle.Common;


namespace CorporateActionsImportTests
{
    [TestClass]
    public class UnitTest1
    {
        private static Importer s_importer;
        private static PrivateObject s_privateObject;
        private static CommandLineParameters m_parms = new CommandLineParameters();
 
        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            m_parms.Reader = "RealTick";
            m_parms.Writer = "Log";
            m_parms.TaskName = "CorporateActionsUpdate";

            s_importer = new Importer(m_parms);
            s_privateObject = new PrivateObject(s_importer);
        }

        private static void Utilities_OnError(object sender, LoggingEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(e.Message + "=>" + e.Exception.Message);
        }
        private static void Utilities_OnInfo(object sender, LoggingEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(e.Message);
        }
        [ClassCleanup]
        public static void Cleanup()
        {
            if (s_importer != null)
            {
                s_importer.Dispose();
                s_importer = null;
            }
        }


         [TestMethod]
        public void TestGetGetUnderlyings()
        {
            int expected = 5;
            object objResult = s_privateObject.Invoke("GetUnderlyings");
            string[] list = (string[])objResult;
            int actual = list.Length;

            Assert.AreEqual(expected, actual, "GetUnderlyings failed");
        }

        [TestMethod]
        public void TestStartTask()
        {
            bool expected = true;
            object objResult = s_privateObject.Invoke("StartTask");
            bool actual = (bool)objResult;

            Assert.AreEqual(expected, actual, "StartTask failed");
        }

        [TestMethod]
        public void TestImport()
        {
            m_parms.NumberOfDays = 8;
            bool expected = true;
            bool actual = s_importer.Run();

            Assert.AreEqual(expected, actual, "Import failed");
        }

        [TestMethod]
        public void TestImportAll()
        {
            m_parms.NumberOfDays = -1;
            bool expected = true;
            bool actual = s_importer.Run();

            Assert.AreEqual(expected, actual, "Import failed");
        }

        [TestMethod]
        public void TestTWS()
        {
            m_parms.Reader = "TWS";
            m_parms.RetryCount = 0;
            bool expected = true;
            bool actual = s_importer.Run();

            Assert.AreEqual(expected, actual, "Import failed");
        }
    }
}
