using System;
using Unity.GraphToolkit.Editor;

namespace LogicGraph.Editor.Nodes.ContextNodes;

/// <summary>
/// This is a context node for gameObject related functionality block nodes.
/// This is just a sample use case of context nodes. It structures gameObject related operations to this context instead of the global context which can improve readability of the graph.
/// </summary>
[Serializable]
public class GameObjectContextNode : ContextNode
{
    public UnityEngine.Object Source;

    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddInputPort<UnityEngine.GameObject>(nameof(Source))
               .Build();
    }
}