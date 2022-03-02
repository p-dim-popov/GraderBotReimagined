using System;
using System.IO;
using NUnit.Framework;
using Runners.Abstractions;

namespace Runners.Tests;

public class BaseUnitTests
{
    private readonly Func<ITestableApp> _createTestableApp;
    protected ITestableApp _testableApp = null!;
    protected DirectoryInfo _solutionDir = null!;
    private const string TempDir = "grader-bot-tests";

    public BaseUnitTests(Func<ITestableApp> createTestableApp)
    {
        _createTestableApp = createTestableApp;
    }

    [SetUp]
    public void Setup()
    {
        _testableApp = _createTestableApp();
        _solutionDir = Directory.CreateDirectory(
            Path.Join(Path.GetTempPath(), TempDir, $"{DateTime.Now:yyyy_MM_dd-hh_mm_ss_fff}--{Guid.NewGuid()}")
        );
    }

    [TearDown]
    public void Clean()
    {
        _solutionDir.Delete(true);
        _solutionDir = null!;
        _testableApp = null!;
    }
}
