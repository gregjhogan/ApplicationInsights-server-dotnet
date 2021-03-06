﻿namespace Functional
{
    using Helpers;
    using IisExpress;
    using Microsoft.Developer.Analytics.DataCollection.Model.v2;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;

    [TestClass]
    public class DiagnosticsTest : SingleWebHostTestBase
    {
        private const string TestWebApplicaionSourcePath = @"..\TestApps\AspNetDiagnostics\App";
        private const string TestWebApplicaionDestPath = "TestApps_AspNetDiagnostics_App";

        private const int TestRequestTimeoutInMs = 15000;
        private const int TestListenerTimeoutInMs = 10000;

        private const string DiagnosticsInstrumentationKey = "E2EBDB26-0553-44A4-A4B6-BB35583EA322";

        [TestInitialize]
        public void TestInitialize()
        {
            var applicationDirectory = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    TestWebApplicaionDestPath);

            Trace.WriteLine("Application directory:" + applicationDirectory);

            this.StartWebAppHost(
                new SingleWebHostTestConfiguration(
                    new IisExpressConfiguration
                    {
                        ApplicationPool = IisExpressAppPools.Clr4IntegratedAppPool,
                        Path = applicationDirectory,
                        Port = 42355,
                    })
                {
                    TelemetryListenerPort = 4000
                });
        }

        [TestCleanup]
        public void TestCleanUp()
        {
            this.StopWebAppHost(false);
        }

        [TestMethod]
        [Owner("sergeyni")]
        [Description("Validates that diagnostics module sends trace data to the Portal")]
        [DeploymentItem(TestWebApplicaionSourcePath, TestWebApplicaionDestPath)]
        public void TestDiagnosticsFW45()
        {
            var responseTask = this.HttpClient.GetStringAsync("/");

            responseTask.Wait(TestRequestTimeoutInMs);
            
            Assert.IsTrue(responseTask.Result.Contains("Home Page - My ASP.NET Application"), "Incorrect response returned: " + responseTask.Result);

            var items = Listener.ReceiveAllItemsDuringTimeOfType<TelemetryItem<MessageData>>(TestListenerTimeoutInMs);
            
            // Check that instrumentation key is correct
            Assert.AreEqual(0, items.Count(i => !i.IKey.Equals(DiagnosticsInstrumentationKey)), "Some item does not have DiagnosticsInstrumentationKey");

            // There should be one custom actionable event about incorrect timeout of session expiration
            Assert.IsTrue(items.Count(i => i.Data.BaseData.Message.StartsWith("AI: ")) == 1, "AI actionable event was not recieved");
        }
    }
}
