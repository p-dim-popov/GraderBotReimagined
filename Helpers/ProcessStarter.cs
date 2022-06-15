using System.Diagnostics;
using Core.Types;
using Core.Utilities;

namespace Helpers;

public class ProcessStarter: IProcessStarter
{
    public Result<Process, object> Start(string program, IEnumerable<string>? args, Dictionary<string, string>? environmentVariables)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = $"{program}",
                Arguments = $"{args?.StringJoin(" ")}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            },
            EnableRaisingEvents = true,
        };
        if (environmentVariables is {} envVars)
        {
            foreach (var (key, value) in envVars)
            {
                process.StartInfo.EnvironmentVariables[key] = value;
            }
        }

        var startResult = Ops.RunCatching(process.Start);

        if (startResult is None<bool, Exception> errorStartResult)
        {
            throw errorStartResult.Error;
        }

        return new Some<Process, object>(process);
    }
}
