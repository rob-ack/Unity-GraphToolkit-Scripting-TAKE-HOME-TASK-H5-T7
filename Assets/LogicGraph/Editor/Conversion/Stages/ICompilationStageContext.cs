using System;
using Unity.GraphToolkit.Editor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LogicGraph.Editor.Conversion;

public interface ICompilationStageContext
{
    public Object AssetContext { get; }
    public Graph Graph { get; }
    public ILogHandler Logger { get; }

    /// <summary>
    /// Add an operation that will be called after all nodes have been compiled.
    /// This allows for finalisation as all nodes can now be found in the context and linked together.
    /// </summary>
    void AddLinkStageFinalisation(Action<ILinkStageContext> action);
}