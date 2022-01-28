using System.Diagnostics;
using Utilities;

namespace Contracts;

public interface IProcessStarter
{
    Result<Process, bool> Start(string program, params string[]? args);
}
