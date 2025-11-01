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
        static string GetResultsPath()
        {
            var path = Environment.GetEnvironmentVariable("CB_TEST_RESULTS_PATH");
            if (string.IsNullOrEmpty(path)) path = "TestResults/PlayModeResults.xml";
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            return path.Replace('\\','/');
        }

        static void WriteMinimalNUnitXml(string filePath, int testCount, int failCount, string message)
        {
            var now = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");
            var resultAttr = failCount > 0 ? "Failed" : (testCount > 0 ? "Passed" : "Inconclusive");
            var passed = Math.Max(0, testCount - failCount);
            var safeMsg = System.Security.SecurityElement.Escape(message ?? string.Empty);
            var xml = $"<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                      $"<test-run id=\"2\" name=\"PlayMode\" testcasecount=\"{testCount}\" result=\"{resultAttr}\" total=\"{testCount}\" passed=\"{passed}\" failed=\"{failCount}\" inconclusive=\"0\" skipped=\"0\" asserts=\"0\" start-time=\"{now}\" end-time=\"{now}\">\n" +
                      $"  <command-line>CI.PlayModeCiRunner.Run</command-line>\n" +
                      $"  <test-suite type=\"TestSuite\" name=\"PlayMode\" result=\"{resultAttr}\" total=\"{testCount}\" executed=\"True\" passed=\"{passed}\" failed=\"{failCount}\" inconclusive=\"0\" skipped=\"0\" asserts=\"0\">\n" +
                      $"    <failure><message>{safeMsg}</message></failure>\n" +
                      $"  </test-suite>\n" +
                      $"</test-run>";
            File.WriteAllText(filePath, xml);
        }

        [MenuItem("CI/Run PlayMode Tests (CI)")]
        public static void Run()
        {
            var api = ScriptableObject.CreateInstance<TestRunnerApi>();

            var filter = new Filter
            {
                testMode = TestMode.PlayMode
                // Optional: testNames = new[] { "SamplePlaymodeTests.PlaymodeTest_Passes" }
            };

            var resultsPath = GetResultsPath();
            var exitCode = 0;

            api.RegisterCallbacks(new Callbacks(result =>
            {
                try
                {
                    // Write XML from adaptor
                    WriteMinimalNUnitXml(result, resultsPath);
                    Debug.Log($"[CI] (PlayMode) Wrote NUnit XML to: {resultsPath}");

                    int total = result.PassCount + result.FailCount + result.SkipCount + result.InconclusiveCount;
                    if (total == 0)
                    {
                        Debug.LogError("[CI] (PlayMode) No tests discovered (total == 0). Marking as exit code 2.");
                        // If adaptor write produced nothing, ensure minimal XML exists
                        if (!File.Exists(resultsPath))
                            WriteMinimalNUnitXml(resultsPath, 0, 0, "No PlayMode tests discovered.");
                        exitCode = 2; // explicit 0-tests
                    }
                    else
                    {
                        exitCode = result.FailCount > 0 ? 1 : 0;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[CI] (PlayMode) Exception while writing XML: {ex}");
                    // Fallback minimal XML to guarantee artifact
                    try { WriteMinimalNUnitXml(resultsPath, 0, 1, "Internal error: " + ex.GetType().Name + " - " + ex.Message); }
                    catch { /* swallow */ }
                    exitCode = 1;
                }

                // As a final guard, ensure XML exists
                if (!File.Exists(resultsPath))
                {
                    try { WriteMinimalNUnitXml(resultsPath, 0, 0, "No PlayMode tests discovered."); }
                    catch { /* swallow */ }
                    if (exitCode == 0) exitCode = 2;
                }

                EditorApplication.Exit(exitCode);
            }));

            api.Execute(new ExecutionSettings(filter));
        }

        private static void WriteMinimalNUnitXml(ITestResultAdaptor root, string path)
        {
            using var w = XmlWriter.Create(path, new XmlWriterSettings { Indent = true });
            int total = root.PassCount + root.FailCount + root.SkipCount + root.InconclusiveCount;

            w.WriteStartDocument();
            w.WriteStartElement("test-run");
            w.WriteAttributeString("result", root.ResultState.ToString());
            w.WriteAttributeString("total", total.ToString());
            w.WriteAttributeString("passed", root.PassCount.ToString());
            w.WriteAttributeString("failed", root.FailCount.ToString());
            w.WriteAttributeString("skipped", root.SkipCount.ToString());
            w.WriteAttributeString("inconclusive", root.InconclusiveCount.ToString());

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
