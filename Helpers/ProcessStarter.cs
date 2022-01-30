using System.Diagnostics;
using Core.Types;
using Core.Utilities;

namespace Helpers;

public class ProcessStarter: IProcessStarter
{
    public Result<Process, bool> Start(string program, params string[]? args)
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

        var isStartSuccess = process.Start();
        return isStartSuccess
            ? new SuccessResult<Process, bool>(process)
            : new ErrorResult<Process, bool>(false);
    }
}