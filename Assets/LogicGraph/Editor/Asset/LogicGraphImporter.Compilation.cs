using System;
using System.Linq;
using LogicGraph.Editor.Conversion;
using LogicGraph.Editor.Nodes;
using Unity.GraphToolkit.Editor;

namespace LogicGraph.Editor.Asset;

internal sealed partial class LogicGraphImporter
{
    /// <summary>
    /// Converts a <see cref="INode"/> to a list of one or more runtime <see cref="IRuntimeNodeInstance"/>s and/or <see cref="IRuntimeNodeExecutor"/> operations.
    /// </summary>
    /// <remarks>
    /// This conversion is not always 1:1 and additional operations may be added to the runtime execution.
    /// </remarks>
    private static void CompileNodesToRuntimeNodes(INode nodeModel, ICompilationStageContext compilationContext, IExecutionRootBuilder executionBuilder)
    {
        {
            if (nodeModel is IRuntimeNodeConverter compileableNode)
            {
                var scope = new ExecutionBlockBuilder(executionBuilder, compilationContext);
                compileableNode.Convert(compilationContext, scope);
                var runtimeExecutor = scope.Build();
                executionBuilder.AddExecutionOperation(runtimeExecutor, nodeModel);

                if (nodeModel is StartNode)
                {
                    executionBuilder.WithStartNode(runtimeExecutor);
                }
            }
        }
        {
            if (nodeModel is ContextNode contextNode)
            {
                foreach (var compileableNode in contextNode.blockNodes.OfType<IRuntimeNodeConverter>())
                {
                    var scope = new ExecutionBlockBuilder(executionBuilder, compilationContext);
                    compileableNode.Convert(compilationContext, scope);
                    var runtimeBlockExecution = scope.Build();
                    executionBuilder.AddExecutionOperation(runtimeBlockExecution, compileableNode as BlockNode);
                }
            }
        }
    }
}