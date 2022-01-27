using System.Diagnostics;
using Utilities;

namespace Contracts;

public interface IProgramRunner
{
    Process Run(string program, params string[]? args);
}
