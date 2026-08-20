using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

/// <summary>
/// Headless validation of a recovered Unity project.
///
/// The app under repair renders black on Quest. Extraction cannot recover compiled shaders,
/// so materials commonly come back pointing at nothing and render black; a missing render
/// pipeline asset or a broken camera produces the same symptom. These checks assert those
/// conditions directly, which is stronger and more reproducible evidence than eyeballing a
/// screenshot — and it works with no GPU and no headset.
///
/// Run: Unity -batchmode -quit -projectPath P -executeMethod RebuildValidation.ValidateAll
/// </summary>
public static class RebuildValidation
{
    private const string ErrorShaderName = "Hidden/InternalErrorShader";
    private const string ReportFileName = "validation-report.md";

    public static void ValidateAll()
    {
        var report = new StringBuilder();
        report.AppendLine("# Rebuild validation report");
        report.AppendLine();
        report.AppendLine($"Unity {Application.unityVersion}");
        report.AppendLine();

        var failures = 0;
        try
        {
            failures += ValidateRenderPipeline(report);
            failures += ValidateMaterials(report);
            failures += ValidateScenes(report);
        }
        catch (Exception e)
        {
            report.AppendLine($"## FATAL\n\n```\n{e}\n```");
            failures++;
        }

        report.AppendLine();
        report.AppendLine(failures == 0
            ? "## RESULT: PASS"
            : $"## RESULT: FAIL ({failures} problem group(s))");

        var path = Path.Combine(Directory.GetCurrentDirectory(), ReportFileName);
        File.WriteAllText(path, report.ToString());
        Debug.Log($"[Validation] report written to {path}");
        Console.WriteLine(report.ToString());

        EditorApplication.Exit(failures == 0 ? 0 : 1);
    }

    /// <summary>A missing pipeline asset on a URP project renders nothing at all.</summary>
    private static int ValidateRenderPipeline(StringBuilder report)
    {
        report.AppendLine("## Render pipeline");
        report.AppendLine();

        var failures = 0;
        var defaultPipeline = GraphicsSettings.defaultRenderPipeline;
        report.AppendLine($"- Graphics default pipeline: `{Describe(defaultPipeline)}`");
        if (defaultPipeline == null)
        {
            report.AppendLine("  - **FAIL** no pipeline asset assigned in Graphics settings.");
            failures++;
        }

        var levels = QualitySettings.names;
        for (var i = 0; i < levels.Length; i++)
        {
            QualitySettings.SetQualityLevel(i, applyExpensiveChanges: false);
            var levelPipeline = QualitySettings.renderPipeline;
            var effective = levelPipeline != null ? levelPipeline : defaultPipeline;
            report.AppendLine($"- Quality[{i}] `{levels[i]}`: `{Describe(levelPipeline)}` (effective: `{Describe(effective)}`)");
            if (effective == null)
            {
                report.AppendLine("  - **FAIL** no effective pipeline for this quality level.");
                failures++;
            }
        }

        report.AppendLine();
        return failures;
    }

    /// <summary>Materials whose shader is null or the error shader render black.</summary>
    private static int ValidateMaterials(StringBuilder report)
    {
        report.AppendLine("## Materials");
        report.AppendLine();

        var guids = AssetDatabase.FindAssets("t:Material");
        var broken = new List<string>();
        var byShader = new Dictionary<string, int>();

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                broken.Add($"{path} (material failed to load)");
                continue;
            }

            var shader = material.shader;
            if (shader == null)
            {
                broken.Add($"{path} (shader is null)");
                continue;
            }

            byShader.TryGetValue(shader.name, out var count);
            byShader[shader.name] = count + 1;

            if (shader.name == ErrorShaderName)
            {
                broken.Add($"{path} (shader = {ErrorShaderName})");
            }
        }

        report.AppendLine($"- Materials scanned: {guids.Length}");
        report.AppendLine($"- Broken (null or error shader): **{broken.Count}**");
        report.AppendLine();

        if (broken.Count > 0)
        {
            report.AppendLine("<details><summary>Broken materials</summary>");
            report.AppendLine();
            foreach (var entry in broken.Take(300))
            {
                report.AppendLine($"- `{entry}`");
            }
            if (broken.Count > 300)
            {
                report.AppendLine($"- ...and {broken.Count - 300} more");
            }
            report.AppendLine();
            report.AppendLine("</details>");
            report.AppendLine();
        }

        report.AppendLine("### Shader usage");
        report.AppendLine();
        foreach (var pair in byShader.OrderByDescending(p => p.Value).Take(40))
        {
            report.AppendLine($"- `{pair.Key}` x{pair.Value}");
        }
        report.AppendLine();

        return broken.Count > 0 ? 1 : 0;
    }

    /// <summary>Opens each build scene and asserts something could actually draw it.</summary>
    private static int ValidateScenes(StringBuilder report)
    {
        report.AppendLine("## Scenes");
        report.AppendLine();

        var scenes = EditorBuildSettings.scenes;
        report.AppendLine($"- Scenes in build settings: {scenes.Length}");
        report.AppendLine();

        if (scenes.Length == 0)
        {
            report.AppendLine("- **FAIL** no scenes in build settings.");
            report.AppendLine();
            return 1;
        }

        var failures = 0;
        foreach (var entry in scenes)
        {
            report.AppendLine($"### `{entry.path}` (enabled: {entry.enabled})");
            report.AppendLine();

            if (!File.Exists(entry.path))
            {
                report.AppendLine("- **FAIL** scene file missing on disk.");
                report.AppendLine();
                failures++;
                continue;
            }

            try
            {
                var scene = EditorSceneManager.OpenScene(entry.path, OpenSceneMode.Single);
                var roots = scene.GetRootGameObjects();
                var cameras = roots.SelectMany(r => r.GetComponentsInChildren<Camera>(true))
                                   .Where(c => c.enabled).ToList();
                var missingScripts = CountMissingScripts(roots);

                report.AppendLine($"- Root objects: {roots.Length}");
                report.AppendLine($"- Enabled cameras: {cameras.Count}");
                report.AppendLine($"- Missing script references: {missingScripts}");

                if (cameras.Count == 0)
                {
                    report.AppendLine("- **FAIL** no enabled camera — nothing can render.");
                    failures++;
                }

                foreach (var camera in cameras.Take(5))
                {
                    report.AppendLine($"  - `{camera.name}` clear={camera.clearFlags} " +
                                      $"cullingMask=0x{camera.cullingMask:X} " +
                                      $"near={camera.nearClipPlane} far={camera.farClipPlane}");
                    if (camera.cullingMask == 0)
                    {
                        report.AppendLine("    - **FAIL** culling mask is Nothing.");
                        failures++;
                    }
                }

                if (missingScripts > 0)
                {
                    report.AppendLine($"- **WARN** {missingScripts} missing script reference(s).");
                }
            }
            catch (Exception e)
            {
                report.AppendLine($"- **FAIL** exception opening scene: `{e.Message}`");
                failures++;
            }

            report.AppendLine();
        }

        return failures;
    }

    private static int CountMissingScripts(IEnumerable<GameObject> roots)
    {
        var missing = 0;
        foreach (var root in roots)
        {
            foreach (var component in root.GetComponentsInChildren<Component>(true))
            {
                if (component == null)
                {
                    missing++;
                }
            }
        }
        return missing;
    }

    private static string Describe(RenderPipelineAsset asset)
    {
        return asset == null ? "<none>" : $"{asset.GetType().Name}:{asset.name}";
    }
}
