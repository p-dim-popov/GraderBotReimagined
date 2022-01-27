using System.Diagnostics;
using Contracts;
using Utilities;

namespace Services;

public class ProgramRunner: IProgramRunner
{
    public Process Run(string program, params string[]? args)
    {
        var runner = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = $"{program}",
                Arguments = $"{args?.StringJoin(" ")}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = false,
            },
            EnableRaisingEvents = true,
        };

        runner.Start();
        return runner;
    }
}