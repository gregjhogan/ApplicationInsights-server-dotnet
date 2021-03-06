﻿namespace Unit.Tests
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector.Implementation;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// PerformanceCollector tests.
    /// </summary>
    [TestClass]
    public class PerformanceCollectorTests
    {
        [TestMethod]
        [TestCategory("RequiresPerformanceCounters")]
        public void PerformanceCollectorSanityTest()
        {
            const int CounterCount = 3;
            const string CategoryName = "Processor";
            const string CounterName = "% Processor Time";
            const string InstanceName = "_Total";

            IPerformanceCollector collector = new PerformanceCollector();

            for (int i = 0; i < CounterCount; i++)
            {
                collector.RegisterPerformanceCounter(
                    @"\Processor(_Total)\% Processor Time",
                    null,
                    CategoryName,
                    CounterName,
                    InstanceName,
                    false,
                    true);
            }

            var results = collector.Collect().ToList();

            Assert.AreEqual(CounterCount, results.Count);

            foreach (var result in results)
            {
                var pc = result.Item1.PerformanceCounter;
                var value = result.Item2;

                Assert.AreEqual(CategoryName, pc.CategoryName);
                Assert.AreEqual(CounterName, pc.CounterName);
                Assert.AreEqual(InstanceName, pc.InstanceName);

                Assert.IsTrue(value >= 0 && value <= 100);
            }
        }

        [TestMethod]
        [TestCategory("RequiresPerformanceCounters")]
        public void PerformanceCollectorRefreshTest()
        {
            var counters = new PerformanceCounter[]
                               {
                                   new PerformanceCounter("Processor", "% Processor Time", "_Total"),
                                   new PerformanceCounter("Processor", "% Processor Time", "_Total") 
                               };

            var newCounter = new PerformanceCounter("Memory", "Available Bytes", string.Empty);

            IPerformanceCollector collector = new PerformanceCollector();

            foreach (var pc in counters)
            {
                collector.RegisterPerformanceCounter(
                    PerformanceCounterUtility.FormatPerformanceCounter(pc), 
                    null,
                    pc.CategoryName,
                    pc.CounterName,
                    pc.InstanceName,
                    false,
                    true);
            }

            collector.RefreshPerformanceCounter(collector.PerformanceCounters.Last(), newCounter);

            Assert.IsTrue(collector.PerformanceCounters.Last().PerformanceCounter.CategoryName == newCounter.CategoryName);
            Assert.IsTrue(collector.PerformanceCounters.Last().PerformanceCounter.CounterName == newCounter.CounterName);
            Assert.IsTrue(collector.PerformanceCounters.Last().PerformanceCounter.InstanceName == newCounter.InstanceName);
        }

        [TestMethod]
        [TestCategory("RequiresPerformanceCounters")]
        public void PerformanceCollectorBadStateTest()
        {
            var counters = new PerformanceCounter[]
                               {
                                   new PerformanceCounter("Processor", "% Processor Time", "_Total123blabla"),
                                   new PerformanceCounter("Processor", "% Processor Time", "_Total") 
                               };

            IPerformanceCollector collector = new PerformanceCollector();

            foreach (var pc in counters)
            {
                try
                {
                    collector.RegisterPerformanceCounter(
                        PerformanceCounterUtility.FormatPerformanceCounter(pc),
                        null,
                        pc.CategoryName,
                        pc.CounterName,
                        pc.InstanceName,
                        false,
                        true);
                }
                catch (Exception)
                {
                }
            }

            Assert.IsTrue(collector.PerformanceCounters.First().IsInBadState);
            Assert.IsFalse(collector.PerformanceCounters.Last().IsInBadState);
        }
    }
}