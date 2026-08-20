using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Wires up the recovered project so it can actually render.
///
/// Asset extraction recovered the original pipeline assets (URP-Performant, URP-Balanced,
/// URP-HighFidelity) but could not recover the *references* to them: Graphics settings and
/// every Quality level came back empty. A URP project with no pipeline asset assigned renders
/// nothing at all — audio and game logic run while the screen stays black.
///
/// The recovered asset names correspond exactly to the three recovered quality level names,
/// so each level is matched to its own asset rather than collapsing them onto one.
///
/// Run: Unity -batchmode -quit -projectPath P -executeMethod ConfigureProject.Apply
/// </summary>
public static class ConfigureProject
{
    private const string PipelineAssetFolder = "Assets/MonoBehaviour";
    private const string DefaultPipelineAssetName = "URP-Balanced";
    private const string TmpEssentialsPackage =
        "Packages/com.unity.textmeshpro/Package Resources/TMP Essential Resources.unitypackage";

    public static void Apply()
    {
        try
        {
            ImportTextMeshProEssentials();

            var assigned = AssignPipelinePerQualityLevel();
            AssignDefaultPipeline();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[ConfigureProject] OK — {assigned} quality level(s) wired to a pipeline asset.");
            EditorApplication.Exit(0);
        }
        catch (Exception e)
        {
            Debug.LogError($"[ConfigureProject] FAILED: {e}");
            EditorApplication.Exit(1);
        }
    }

    /// <summary>
    /// TextMeshPro's runtime shaders ship in a .unitypackage rather than the package cache, so
    /// the 23 remapped TMP materials stay unresolved until it is imported.
    /// </summary>
    private static void ImportTextMeshProEssentials()
    {
        if (Directory.Exists("Assets/TextMesh Pro/Shaders"))
        {
            Debug.Log("[ConfigureProject] TMP essentials already present.");
            return;
        }

        if (!File.Exists(TmpEssentialsPackage))
        {
            Debug.LogWarning($"[ConfigureProject] TMP essentials package not found at {TmpEssentialsPackage}");
            return;
        }

        AssetDatabase.ImportPackage(TmpEssentialsPackage, interactive: false);
        AssetDatabase.Refresh();
        Debug.Log("[ConfigureProject] imported TMP Essential Resources.");
    }

    private static RenderPipelineAsset FindPipelineAsset(string assetName)
    {
        var path = $"{PipelineAssetFolder}/{assetName}.asset";
        var asset = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(path);
        if (asset != null)
        {
            return asset;
        }

        // Fall back to a project-wide search so a relocated asset still resolves.
        var guid = AssetDatabase.FindAssets($"{assetName} t:RenderPipelineAsset").FirstOrDefault();
        return guid == null
            ? null
            : AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(AssetDatabase.GUIDToAssetPath(guid));
    }

    private static int AssignPipelinePerQualityLevel()
    {
        var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/QualitySettings.asset");
        if (assets == null || assets.Length == 0)
        {
            throw new InvalidOperationException("Could not load ProjectSettings/QualitySettings.asset");
        }

        var so = new SerializedObject(assets[0]);
        var levels = so.FindProperty("m_QualitySettings");
        if (levels == null || !levels.isArray)
        {
            throw new InvalidOperationException("m_QualitySettings array not found");
        }

        var assignedCount = 0;
        for (var i = 0; i < levels.arraySize; i++)
        {
            var level = levels.GetArrayElementAtIndex(i);
            var levelName = level.FindPropertyRelative("name")?.stringValue ?? $"Level{i}";
            var custom = level.FindPropertyRelative("customRenderPipeline");
            if (custom == null)
            {
                Debug.LogWarning($"[ConfigureProject] quality level '{levelName}' has no customRenderPipeline property");
                continue;
            }

            // "High Fidelity" -> "URP-HighFidelity"
            var assetName = "URP-" + levelName.Replace(" ", string.Empty);
            var pipeline = FindPipelineAsset(assetName) ?? FindPipelineAsset(DefaultPipelineAssetName);

            if (pipeline == null)
            {
                Debug.LogError($"[ConfigureProject] no pipeline asset found for quality level '{levelName}'");
                continue;
            }

            custom.objectReferenceValue = pipeline;
            assignedCount++;
            Debug.Log($"[ConfigureProject] quality[{i}] '{levelName}' -> {pipeline.name}");
        }

        so.ApplyModifiedPropertiesWithoutUndo();
        return assignedCount;
    }

    private static void AssignDefaultPipeline()
    {
        var pipeline = FindPipelineAsset(DefaultPipelineAssetName);
        if (pipeline == null)
        {
            throw new InvalidOperationException($"Default pipeline asset '{DefaultPipelineAssetName}' not found.");
        }

        var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset");
        if (assets == null || assets.Length == 0)
        {
            throw new InvalidOperationException("Could not load ProjectSettings/GraphicsSettings.asset");
        }

        var so = new SerializedObject(assets[0]);
        var property = so.FindProperty("m_CustomRenderPipeline");
        if (property == null)
        {
            throw new InvalidOperationException("m_CustomRenderPipeline property not found");
        }

        property.objectReferenceValue = pipeline;
        so.ApplyModifiedPropertiesWithoutUndo();
        Debug.Log($"[ConfigureProject] Graphics default pipeline -> {pipeline.name}");
    }
}
