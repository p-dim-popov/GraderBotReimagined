using System.Collections.Specialized;
using System.Diagnostics;
using Core.Types;
using Core.Utilities;

namespace Helpers;

public class ProcessStarter: IProcessStarter
{
    public Result<Process, bool> Start(string program, IEnumerable<string>? args, Dictionary<string, string>? environmentVariables)
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

        var isStartSuccess = process.Start();
        return isStartSuccess
            ? new SuccessResult<Process, bool>(process)
            : new ErrorResult<Process, bool>(false);
    }
}
