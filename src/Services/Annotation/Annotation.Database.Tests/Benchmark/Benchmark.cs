using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PreciPoint.Ims.Services.Annotation.Database.Tests.Benchmark;

[TestFixture]
[Order(1)]
[NonParallelizable]
[Category("Benchmark")]
public class Benchmark
{
    [OneTimeTearDown]
    public void PrintResults()
    {
        if (_summary is not null)
        {
            Assert.NotNull(_summary.ResultsDirectoryPath);
            FileAssert.Exists(_summary.LogFilePath);

            IEnumerable<string> filesPath = Directory.EnumerateFiles(_summary.ResultsDirectoryPath, "*.*", SearchOption.AllDirectories)
                .Where(s => s.EndsWith(".md"));

            foreach (string filePath in filesPath)
            {
                FileAssert.Exists(filePath);

                string nameNoExt = Path.GetFileNameWithoutExtension(filePath);
                string fileAllText = File.ReadAllText(filePath);

                TestContext.Progress.WriteLine(
                    "########################################################################");

                TestContext.Progress.WriteLine($"RESULTS: {nameNoExt}");
                TestContext.Progress.WriteLine(fileAllText);

                TestContext.Progress.WriteLine("#");
                TestContext.Progress.WriteLine("#");
            }
        }
    }

    private Summary _summary;

    [Test]
    [Order(1)]
    public void B001_001Queries()
    {
        _summary = BenchmarkRunner.Run<JobQueries>(
            ManualConfig
                .Create(DefaultConfig.Instance)
                .WithOptions(ConfigOptions.DisableOptimizationsValidator));

        Assert.IsNotEmpty(_summary.Reports);
        Assert.IsNotEmpty(_summary.Reports[0].ExecuteResults);
        Assert.AreEqual(0, _summary.Reports[0].ExecuteResults[0].ExitCode);
        FileAssert.Exists(_summary.LogFilePath);
    }
}