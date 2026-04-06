#nullable enable

using System.Threading.Tasks;

namespace LogicGraph.Runtime;

/// <summary>
/// Defines a contract for executors not relying on instance data itself to run but may provide an instance to operate on.
/// </summary>
public interface IRuntimeBlockExecutor
{
    ValueTask ExecuteAsync(IRuntimeContext context);

    IRuntimeNodeInstance? Instance { get; }
}