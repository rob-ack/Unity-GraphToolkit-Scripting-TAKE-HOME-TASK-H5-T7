using LogicGraph.Runtime;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LogicGraph.Editor
{
    [CustomEditor(typeof(LogicRuntimeGraphDirector))]
    public class LogicRuntimeGraphDirectorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var director = this.target as LogicRuntimeGraphDirector;

            if (director)
            {
                using var scope = new EditorGUILayout.HorizontalScope();
                if (GUILayout.Button("Export"))
                {
                    try
                    {
                        var json = JsonUtility.ToJson(director.RuntimeGraph, true);
                        var jsonAsset = new TextAsset(json);
                        jsonAsset.hideFlags = HideFlags.None;

                        string assetPath = AssetDatabase.GetAssetPath(director.RuntimeGraph);

                        var jsonPath = System.IO.Path.ChangeExtension(assetPath, "runtime.json");
                        System.IO.File.WriteAllText(jsonPath, json);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"Failed to serialize runtime asset to JSON:\n{e}");
                    }
                }
                if (GUILayout.Button("Import"))
                {
                    try
                    {
                        string assetPath = AssetDatabase.GetAssetPath(director.RuntimeGraph);
                        assetPath = Path.GetDirectoryName(assetPath);
                        string file = EditorUtility.OpenFilePanelWithFilters("Import JSON", assetPath, new [] { "JSON files", "json" });
                        if (File.Exists(file))
                        {
                            var json = File.ReadAllText(file);
                            var importedGraph = ScriptableObject.CreateInstance<LogicRuntimeGraph>();
                            JsonUtility.FromJsonOverwrite(json, importedGraph);
                            director.RuntimeGraph = importedGraph;
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"Failed to deserialize JSON to runtime asset:\n{e}");
                    }
                }
            }
            DrawDefaultInspector();
        }
    }
}