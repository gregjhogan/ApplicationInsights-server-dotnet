﻿// -----------------------------------------------------------------------
// <copyright file="TestsExcCollectingFw40AspxClassicPipeline.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation. 
// All rights reserved.  2014
// </copyright>
// <author>Sergei Nikitin: sergeyni@microsoft.com</author>
// <summary></summary>
// -----------------------------------------------------------------------

namespace Functional
{
    using Helpers;
    using IisExpress;
    using Microsoft.Developer.Analytics.DataCollection.Model.v2;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Diagnostics;
    using System.IO;
    using System.Net;

    [TestClass]
    public class TestsExcCollectingFw40AspxClassicPipeline : ExceptionTelemetryTestBase
    {
        private const string TestWebApplicaionSourcePath = @"..\TestApps\Wa40Aspx\App";
        private const string TestWebApplicaionDestPath = "TestApps_TestsExcCollectingFw40AspxClassicPipeline_App";

        private const int TestRequestTimeoutInMs = 150000;
        private const int TestListenerTimeoutInMs = 5000;

        [TestInitialize]
        public void TestInitialize()
        {
            var applicationDirectory = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    TestWebApplicaionDestPath);

            Trace.WriteLine("Application directory:" + applicationDirectory);

            File.Copy(
                Path.Combine(applicationDirectory, "App_Data", "ClassicPipeline.Web.config"),
                Path.Combine(applicationDirectory, "Web.config"),
                true);

            this.StartWebAppHost(
                new SingleWebHostTestConfiguration(
                    new IisExpressConfiguration
                    {
                        ApplicationPool = IisExpressAppPools.Clr4ClassicAppPool,
                        Path = applicationDirectory,
                        Port = 31337,
                    })
                {
                    TelemetryListenerPort = 4005,
                    AttachDebugger = Debugger.IsAttached,
                    IKey = "11111111-2222-3333-4444-333333555555",
                });
        }

        [TestCleanup]
        public void TestCleanUp()
        {
            this.StopWebAppHost();
        }

        /// <summary>
        /// Tests exception collecting form sync web page
        /// </summary>
        [Owner("sergeyni")]
        [Description("Tests exception collecting form sync web page")]
        [DeploymentItem(TestWebApplicaionSourcePath, TestWebApplicaionDestPath)]
        [TestMethod]
        [Ignore]
        // Currently we do not collect exceptions in classic mode
        public void TestExceptionWebFormInClassicMode()
        {
            const string RequestPath = "/SyncExceptionWebForm.aspx";
            const string ContentMarker =
                "SyncExceptionWebForm: NotImplemented";

            var responseTask = this.HttpClient.GetAsync(RequestPath);

            Assert.IsTrue(
                responseTask.Wait(TestRequestTimeoutInMs),
                "Request was not executed in time");

            Assert.IsFalse(
                responseTask.Result.IsSuccessStatusCode,
                "Request succeeded");

            Assert.AreEqual(
                HttpStatusCode.InternalServerError,
                responseTask.Result.StatusCode,
                "Unexpected response code");

            var responseData = responseTask.Result.Content.ReadAsStringAsync().Result;
            Trace.Write(responseData);

            Assert.IsTrue(
                responseData.Contains(ContentMarker),
                "Exception description does not contain expected data: {0}",
                responseData);

            var items = Listener
                .ReceiveItemsOfTypes<TelemetryItem<RequestData>, TelemetryItem<ExceptionData>>(TestListenerTimeoutInMs, 2);

            // One item is request, the other one is exception.
            int requestItemIndex = (items[0] is TelemetryItem<RequestData>) ? 0 : 1;
            int exceptionItemIndex = (requestItemIndex == 0) ? 1 : 0;

            var exceptionItem = (TelemetryItem<ExceptionData>)items[exceptionItemIndex];
            this.ValidateExceptionTelemetry(
                exceptionItem,
                (TelemetryItem<RequestData>)items[requestItemIndex],
                2);

            Trace.WriteLine("Validate details 0");
            this.ValidateExceptionDetails(
                exceptionItem.Data.BaseData.Exceptions[0],
                "System.Web.HttpUnhandledException",
                "Exception of type 'System.Web.HttpUnhandledException' was thrown.",
                "System.Web.UI.Page.HandleError",
                "System.Web, Version=", 
                9);

            Trace.WriteLine("Validate details 1");
            this.ValidateExceptionDetails(
                exceptionItem.Data.BaseData.Exceptions[1],
                "System.NotImplementedException",
                "SyncExceptionWebForm: NotImplemented",
                "Wa40Aspx.SyncExceptionWebForm.Page_Load",
                "Wa40Aspx, Version=",
                5);
        }
    }
}
