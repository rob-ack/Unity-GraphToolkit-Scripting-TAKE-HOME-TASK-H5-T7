#nullable enable

using UnityEngine;

namespace LogicGraph.Runtime;

public interface IRuntimeContext
{
    IExposedPropertyTable ExposedPropertyTable { get; }
    LogicRuntimeGraphDirector GraphDirector { get; }
}