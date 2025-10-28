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
        [MenuItem("CI/Run PlayMode Tests (CI)")]
        public static void Run()
        {
            var api = ScriptableObject.CreateInstance<TestRunnerApi>();

            var filter = new Filter
            {
                testMode = TestMode.PlayMode
                // Optional: testNames = new[] { "SamplePlaymodeTests.PlaymodeTest_Passes" }
            };

            var outputPath = Environment.GetEnvironmentVariable("CB_TEST_RESULTS_PATH");
            if (string.IsNullOrEmpty(outputPath))
                outputPath = "TestResults/PlayModeResults.xml";

            var dir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

            var exitCode = 0;

            api.RegisterCallbacks(new Callbacks(result =>
            {
                try
                {
                    // Write XML always, even if 0 tests discovered
                    WriteMinimalNUnitXml(result, outputPath);
                    Debug.Log($"[CI] (PlayMode) Wrote NUnit XML to: {outputPath}");

                    int total = result.PassCount + result.FailCount + result.SkipCount + result.InconclusiveCount;
                    if (total == 0)
                    {
                        Debug.LogError("[CI] (PlayMode) No tests discovered (total == 0). Failing with exit code 2.");
                        exitCode = 2; // explicit 0-tests fail
                    }
                    else
                    {
                        exitCode = result.FailCount > 0 ? 1 : 0;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[CI] (PlayMode) Failed to write XML: {ex}");
                    exitCode = 1;
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

