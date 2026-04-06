using System;
using System.Linq;
using LogicGraph.Editor.Nodes;
using LogicGraph.Runtime;
using Unity.GraphToolkit.Editor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace LogicGraph.Editor.Asset
{
    /// <summary>
    /// LogicGraphImporter is a <see cref="ScriptedImporter"/> that imports the <see cref="LogicEditorGraphAsset"/>
    /// and builds the corresponding <see cref="LogicRuntimeGraph"/> asset.
    /// </summary>
    [ScriptedImporter(1, LogicEditorGraphAsset.AssetExtension)]
    internal sealed partial class LogicGraphImporter : ScriptedImporter
    {
        /// <summary>
        /// Unity calls this method when the editor imports the asset. This method then processes the imported <see cref="LogicEditorGraphAsset"/>.
        /// </summary>
        /// <param name="ctx">The asset import context.</param>
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var graph = GraphDatabase.LoadGraphForImporter<LogicEditorGraphAsset>(ctx.assetPath);

            // The `graph` may be null if the `GraphDatabase.LoadGraphForImporter` method
            // fails to load the asset from the specified `ctx.assetPath`.
            // This can occur under the following circumstances:
            // - The asset path is incorrect, or the asset does not exist at the specified location.
            // - The asset located at the specified path is not of type `VisualNovelDirectorGraph`.
            // - The asset file itself is problematic. For example, it is corrupted, or stored in an unsupported format.
            //
            // Best practice to deal with serialization is to thoroughly validate and safeguard against
            // impaired or incomplete data, to account for potential deserialization issues.
            if (graph == null)
            {
                ctx.LogImportError($"Failed to load Logic Graph asset: {ctx.assetPath}");
                return;
            }

            // Get the first Start Node
            // (Only using the first node is a simplification we made for this sample)
            var startNodeModel = graph.GetNodes().OfType<StartNode>().FirstOrDefault();
            if (startNodeModel == null)
            {
                //TODO: the start node is never present when the graph was just created.
                ctx.LogImportWarning($"Failed to find Start Node in Logic Graph asset: {ctx.assetPath}");
                return;
            }

            // Build the runtime asset by iterating the graph and converting nodes to runtime representation.
            var runtimeAsset = ScriptableObject.CreateInstance<LogicRuntimeGraph>();

            {
                var compilationContext = new CompilationContextBuilder(graph, ctx, runtimeAsset);
                foreach (var node in graph.GetNodes())
                {
                    CompileNodesToRuntimeNodes(node, compilationContext, compilationContext);
                }
                compilationContext.FinishCompilation();
            }

            // Add the runtime object to the graph asset and set it to be the main asset.
            // This allows the same asset to be used in inspectors wherever a runtime asset is expected.
            ctx.AddObjectToAsset("RuntimeAsset", runtimeAsset);
            ctx.SetMainObject(runtimeAsset);

            try
            {
                //serialize to json with newtonsoft with references preserved to avoid duplicated references. such json could be loaded back.
                var json = JsonUtility.ToJson(runtimeAsset, true);
                var jsonAsset = new TextAsset(json);
                //make visible in inspector for debugging purposes
                jsonAsset.hideFlags = HideFlags.None;
                //save to disk for debugging purposes
                var jsonPath = System.IO.Path.ChangeExtension(ctx.assetPath, "runtime.json");
                System.IO.File.WriteAllText(jsonPath, json);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to serialize runtime asset to JSON: {e.Message}");
            }
        }
    }
}