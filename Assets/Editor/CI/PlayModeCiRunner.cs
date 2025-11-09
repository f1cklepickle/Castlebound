using System;
using System.IO;
using System.Xml;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;

namespace CI
{
    /// <summary>
    /// CI entry point for running all PlayMode tests and writing an NUnit-style XML file.
    /// This class is fully self-contained: it creates its own TestRunnerApi instance,
    /// registers callbacks for this run, and writes XML when the run finishes.
    /// </summary>
    public static class PlayModeCiRunner
    {
        static readonly string LogPrefix = "[CI][PlayMode]";
        static TestRunnerApi s_Api;
        static CallbackImpl s_Callbacks;

        static TestRunnerApi GetOrCreateApi()
        {
            if (s_Api != null)
                return s_Api;

            s_Api = ScriptableObject.CreateInstance<TestRunnerApi>();
            s_Callbacks = new CallbackImpl();
            s_Api.RegisterCallbacks(s_Callbacks);

            Debug.Log($"{LogPrefix} TestRunnerApi created and callbacks registered.");
            return s_Api;
        }
        /// <summary>
        /// Determine where to write the test results XML.
        /// CI sets CB_TEST_RESULTS_PATH; otherwise, default to TestResults/PlayModeResults.xml.
        /// </summary>
        internal static string GetResultsPath()
        {
            var path = Environment.GetEnvironmentVariable("CB_TEST_RESULTS_PATH");
            if (string.IsNullOrEmpty(path))
            {
                path = "TestResults/PlayModeResults.xml";
            }

            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }

            // Normalise slashes for external tools.
            return path.Replace('\\', '/');
        }

        /// <summary>
        /// Minimal NUnit-ish XML writer that CI can consume.
        /// </summary>
        internal static void WriteMinimalNUnitXml(ITestResultAdaptor root, string path)
        {
            var total = root.PassCount + root.FailCount + root.SkipCount + root.InconclusiveCount;

            var settings = new XmlWriterSettings
            {
                Indent = true
            };

            using (var w = XmlWriter.Create(path, settings))
            {
                w.WriteStartDocument();

                // Root element with summary attributes.
                w.WriteStartElement("test-run");
                w.WriteAttributeString("result", root.ResultState.ToString());
                w.WriteAttributeString("total", total.ToString());
                w.WriteAttributeString("passed", root.PassCount.ToString());
                w.WriteAttributeString("failed", root.FailCount.ToString());
                w.WriteAttributeString("skipped", root.SkipCount.ToString());
                w.WriteAttributeString("inconclusive", root.InconclusiveCount.ToString());

                // Wrap all test cases in a single "Assembly" suite for simplicity.
                w.WriteStartElement("test-suite");
                w.WriteAttributeString("type", "Assembly");
                w.WriteAttributeString("name", "_Project.Tests.PlayMode");
                w.WriteStartElement("results");

                WriteCasesRecursive(w, root);

                w.WriteEndElement(); // results
                w.WriteEndElement(); // test-suite
                w.WriteEndElement(); // test-run

                w.WriteEndDocument();
            }
        }

        /// <summary>
        /// Menu entry and CI entry point. Creates a fresh TestRunnerApi instance,
        /// registers callbacks for this run, and executes all PlayMode tests.
        /// </summary>
        [MenuItem("CI/Run PlayMode Tests (CI)")]
        public static void Run()
        {
            var apiInstance = GetOrCreateApi();

            var filter = new Filter
            {
                testMode = TestMode.PlayMode
            };

            // In batchmode we *must* block until tests finish, or Unity will quit
            // before RunFinished gets called and our XML is written.
            var settings = new ExecutionSettings(filter)
            {
                runSynchronously = Application.isBatchMode
            };

            Debug.Log($"{LogPrefix} Executing ALL PlayMode tests via TestRunnerApi (no filters, runSynchronously={settings.runSynchronously})...");

            apiInstance.Execute(settings);
        }

        /// <summary>
        /// Called by the callback when the test run finishes.
        /// Writes XML and, in batchmode, exits Unity with an appropriate exit code.
        /// </summary>
        internal static void HandleRunFinished(ITestResultAdaptor result)
        {
            var resultsPath = GetResultsPath();
            var total = result.PassCount + result.FailCount + result.SkipCount + result.InconclusiveCount;
            var exitCode = (total == 0 || result.FailCount > 0) ? 1 : 0;

            try
            {
                WriteMinimalNUnitXml(result, resultsPath);
                Debug.Log($"{LogPrefix} Wrote NUnit XML to: {resultsPath}");
                Debug.Log($"{LogPrefix} Totals -> total={total} passed={result.PassCount} failed={result.FailCount} skipped={result.SkipCount}");

                if (total == 0)
                {
                    Debug.LogWarning($"{LogPrefix} No tests were discovered by TestRunnerApi.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LogPrefix} Failed to write XML: {ex}");
                if (exitCode == 0)
                {
                    exitCode = 1;
                }
            }
            finally
            {
                Debug.Log($"{LogPrefix} RunFinished, exitCode={exitCode}, total={total}, failed={result.FailCount}, skipped={result.SkipCount}.");
                if (Application.isBatchMode)
                {
                    Debug.Log($"{LogPrefix} Exiting batchmode editor with code {exitCode}.");
                    EditorApplication.Exit(exitCode);
                }
            }
        }

        private static void WriteCasesRecursive(XmlWriter w, ITestResultAdaptor node)
        {
            var hasChildren = node.Children != null && node.Children.Any();

            if (!hasChildren)
            {
                w.WriteStartElement("test-case");
                w.WriteAttributeString("name", node.Name ?? (node.Test?.Name ?? "Unnamed"));
                w.WriteAttributeString("result", node.ResultState.ToString());
                w.WriteAttributeString("duration", node.Duration.ToString("0.000"));

                if (!string.IsNullOrEmpty(node.Message))
                {
                    w.WriteElementString("failure", node.Message);
                }

                w.WriteEndElement(); // test-case
                return;
            }

            foreach (var child in node.Children)
            {
                WriteCasesRecursive(w, child);
            }
        }

        /// <summary>
        /// Simple callback implementation that forwards RunFinished to HandleRunFinished.
        /// </summary>
        internal sealed class CallbackImpl : ICallbacks
        {
            public void RunStarted(ITestAdaptor testsToRun) { }

            public void RunFinished(ITestResultAdaptor result)
            {
                PlayModeCiRunner.HandleRunFinished(result);
            }

            public void TestStarted(ITestAdaptor test) { }
            public void TestFinished(ITestResultAdaptor result) { }
        }
    }
}
