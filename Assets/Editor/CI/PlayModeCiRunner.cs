using System;
using System.IO;
using System.Xml;
using System.Linq;
using UnityEngine;                             // ScriptableObject, Debug
using UnityEditor;                             // Editor APIs
using UnityEditor.TestTools.TestRunner.Api;    // TestRunner API

namespace CI
{
    public static class PlayModeCiRunner
    {
        // XML is generated from TestRunnerApi results; no pre-seed needed here
        static string GetResultsPath()
        {
            var path = Environment.GetEnvironmentVariable("CB_TEST_RESULTS_PATH");
            if (string.IsNullOrEmpty(path)) path = "TestResults/PlayModeResults.xml";
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            return path.Replace('\\','/');
        }

        // Writer that mirrors EditMode runner's structure
        private static void WriteMinimalNUnitXml(ITestResultAdaptor root, string path)
        {
            using var w = XmlWriter.Create(path, new XmlWriterSettings { Indent = true });
            w.WriteStartDocument();
            w.WriteStartElement("test-run");
            w.WriteAttributeString("result", root.ResultState.ToString());
            w.WriteAttributeString("total", (root.PassCount + root.FailCount + root.SkipCount + root.InconclusiveCount).ToString());
            w.WriteAttributeString("passed", root.PassCount.ToString());
            w.WriteAttributeString("failed", root.FailCount.ToString());
            w.WriteAttributeString("skipped", root.SkipCount.ToString());
            w.WriteAttributeString("inconclusive", root.InconclusiveCount.ToString());

            w.WriteStartElement("test-suite");
            w.WriteAttributeString("type", "Assembly");
            w.WriteAttributeString("name", "_Project.Tests.PlayMode");
            w.WriteStartElement("results");

            WriteCasesRecursive(w, root);

            w.WriteEndElement();
            w.WriteEndElement();
            w.WriteEndElement();
            w.WriteEndDocument();
        }

        [MenuItem("CI/Run PlayMode Tests (CI)")]
        public static void Run()
        {
            var api = ScriptableObject.CreateInstance<TestRunnerApi>();

            var filter = new Filter
            {
                testMode = TestMode.PlayMode
            };

            var resultsPath = GetResultsPath();

            Callbacks cb = null;
            cb = new Callbacks(result =>
            {
                try
                {
                    WriteMinimalNUnitXml(result, resultsPath);
                    Debug.Log($"[CI][PlayMode] Wrote NUnit XML to: {resultsPath}");
                    var total = result.PassCount + result.FailCount + result.SkipCount + result.InconclusiveCount;
                    Debug.Log($"[CI][PlayMode] Totals -> total={total} passed={result.PassCount} failed={result.FailCount} skipped={result.SkipCount}");
                    if (total == 0)
                    {
                        Debug.LogWarning("[CI][PlayMode] No tests were discovered by TestRunnerApi.");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[CI][PlayMode] Failed to write XML: {ex}");
                }
                finally
                {
                    // Always unregister callbacks when the run finishes
                    api.UnregisterCallbacks(cb);
                }
            });

            api.RegisterCallbacks(cb);

            var settings = new ExecutionSettings(filter);

            Debug.Log("[CI][PlayMode] Executing ALL PlayMode tests via TestRunnerApi (no filters)...");

            if (Application.isBatchMode)
            {
                // In CI / batchmode, run synchronously so Unity quits only after tests are done.
                settings.runSynchronously = true;
                api.Execute(settings);
            }
            else
            {
                // In the Editor, run asynchronously so we don't block the UI.
                api.Execute(settings);
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
                    w.WriteElementString("failure", node.Message);
                w.WriteEndElement();
                return;
            }

            foreach (var child in node.Children)
                WriteCasesRecursive(w, child);
        }

        private sealed class Callbacks : ICallbacks
        {
            private readonly Action<ITestResultAdaptor> onFinished;
            public Callbacks(Action<ITestResultAdaptor> onFinished) => this.onFinished = onFinished;
            public void RunStarted(ITestAdaptor testsToRun) { }
            public void RunFinished(ITestResultAdaptor result) => onFinished?.Invoke(result);
            public void TestStarted(ITestAdaptor test) { }
            public void TestFinished(ITestResultAdaptor result) { }
        }
    }
}
