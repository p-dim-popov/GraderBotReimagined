using System.Collections.Specialized;
using System.Diagnostics;
using Core.Types;

namespace Helpers;

public interface IProcessStarter
{
    Result<Process, object> Start(string program, IEnumerable<string>? args = null, Dictionary<string, string>? environmentVariables = null);
}
