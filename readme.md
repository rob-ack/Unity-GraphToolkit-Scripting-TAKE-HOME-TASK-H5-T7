# Simple Script Graph based on Graph Toolkit

## Introduction

This tool is a **simple implementation of a Graph-based scripting tool**. The goal was to allow creation and manipulation of primitives or prefabs with control over position, rotation, and scale.

The implementation showcases how to use the experimental ``Graph Toolkit 0.4.0-exp2`` package to create a visual graph tool for these operations.

Simple Sample Graph:
![Graph](./docs/SimpleGraphScreenshot.png)

Simple Sample Graph runtime result:
![GraphRun](./docs/SimpleGraphOutput.gif)

## Overview

The implementation follows separation of a ``Design-Time`` and a ``Runtime`` graph data model.

The ``Design-Time`` graph data model uses the ``Graph Toolkit`` package and contains all nodes and their connections, along with constants, variables, portals, and subgraphs.

The ``Runtime`` graph data model is a custom implementation and is a result of Design-Time nodes converted into runtime nodes.
This graph ``conversion`` is performed in a Design-Time compilation step which mainly runs in two stages (convert and link stage). During Runtime a Load stage will prepare additional information for fast performant execution.

***The Conversion stage*** creates one or more operations per Design-Time node which also can perform additional link steps during the link stage.
The operations execute at runtime and drives synchronous or asynchronous logic: for example, Set, Get, Create, If, While, For, Delay, etc.

***The Link stage*** performs additional conversion steps after every design-time node in the graph has been converted. Additional information can be built and stored, such as node relations, member value transfer, execution flow, and event bindings.

***The Runtime Load stage*** is an additional ``Runtime`` stage that precomputes non-serializable operations such as fast ``expression trees``. These expression trees for example dynamically define how data transfer between nodes is handled.

***Runtime Execution*** is performed by a given director ``Monobehaviour`` that starts during play time. It runs the start node each graph defines. The graph then executes as long as nodes continue to execute and ends when all nodes have been executed along graph the defined execution flow.

## Features

- Create Object (primitive, prefab)
- Delay
- Rotate Game Object
- Branching
- Get/Set Game Object position, rotation, scale

## Code Example

This shows how to manually create a design-time node model and provide runtime node conversion for a add rotation to a game object node.
```c#
//This node implements design time data model and conversion logic for runtime node
[Serializable]
class RotateObjectNode : AbstractNode, IRuntimeNodeConverter
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        //add default execution pins
        AddInputOutputExecutionPorts(context);
        //add two input pins
        context.AddInputPort<GameObject>(nameof(RotateObjectRuntimeNode.Target)).Build();
        context.AddInputPort<Vector3>(nameof(RotateObjectRuntimeNode.Rotation)).Build();
    }

    void IRuntimeNodeConverter.Convert(ICompilationStageContext context, IExecutionBuilder executionBuilder)
    {
        //try to resolve constant data from pin inputs
        var targetInputPort = GetInputPortByName(nameof(RotateObjectRuntimeNode.Target));
        bool isTargetInputSet = targetInputPort.TryGetCompileTimeInputPortValue<GameObject>(out var target);

        var rotationInputPort = GetInputPortByName(nameof(RotateObjectRuntimeNode.Rotation));
        bool isRotationInputSet = rotationInputPort.TryGetCompileTimeInputPortValue<Vector3>(out var rotation);
        
        //for non constant inputs register ports to build runtime Get calls for connected target pins
        List<IPort> portsToBind = new ();
        if (!isTargetInputSet)
        {
            portsToBind.Add(targetInputPort);
        }

        if (!isRotationInputSet)
        {
            portsToBind.Add(rotationInputPort);
        }

        //create the runtime implementation
        var node = new RotateObjectRuntimeNode
                   {
                       Target = target,
                       Rotation = rotation,
                   };

        executionBuilder
            //push operation that Get the runtime values from inputs. this can be null and will result in noop if not needed
           .PushExecution(executionBuilder.OperationFactory.CreateRuntimeGetMemberValuesOperation(node, portsToBind.ToArray()))
            //push the runtime implementation execution
           .PushExecution(node)
            //push call to run next node if any
           .PushExecution(executionBuilder.OperationFactory.CreateCallExecutionOperation(GetOutputPortByName(EXECUTION_PORT_DEFAULT_NAME)))
            //set instance data to the runtime node. in this sample execution and instance data is provided by the same implementation but API wise you can separate this.
           .WithInstance(node);
    }
} 
```

This shows how to build the corresponding runtime implementation:
```c#
//class implements both instance data and execution for illustration
public class RotateObjectRuntimeNode : AbstractRuntimeNode, IRuntimeNodeExecutor<RotateObjectRuntimeNode>
{
    //input parameters are filled by runtime operations like 'CreateRuntimeGetMemberValuesOperation' as show above
    public Vector3 Rotation;
    public GameObject Target;

    public ValueTask ExecuteAsync(RotateObjectRuntimeNode nodeInstance, IRuntimeContext context)
    {
        //apply the rotation
        nodeInstance.Target.transform.Rotate(nodeInstance.Rotation);
        //return immediate completion
        return default;
    }
}
```

## Prerequisites

- Unity 6000.3.2f1

## Improvements

- Source Generator to generate nodes for desired APIs
- Refactoring (Assembly definition files, nullables, project cleanup, etc.)
- Documentation design, design choices, problems, and improvements
- Runtime debugging