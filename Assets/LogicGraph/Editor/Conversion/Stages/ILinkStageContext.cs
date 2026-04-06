#nullable enable

using LogicGraph.Runtime;
using Unity.GraphToolkit.Editor;

namespace LogicGraph.Editor.Conversion;

public interface ILinkStageContext
{
    IRuntimeBlockExecutor? GetExecutorInstanceByNode(INode node);
}