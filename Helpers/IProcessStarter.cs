using System.Diagnostics;
using Core.Types;

namespace Helpers;

public interface IProcessStarter
{
    Result<Process, bool> Start(string program, params string[]? args);
}
