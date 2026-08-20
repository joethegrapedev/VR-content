using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Builds the Quest APK and writes a size report.
///
/// The size report exists to test the standing hypothesis that the previous ARMv7/Mono build
/// died of memory exhaustion: the main scene is backed by roughly 1.1 GB of assets, which a
/// 32-bit address space cannot hold. Per-scene and per-asset-type totals make that concrete
/// instead of speculative.
///
/// Run: Unity -batchmode -quit -projectPath P -executeMethod BuildQuestApk.Build
/// </summary>
public static class BuildQuestApk
{
    private const string OutputDirectory = "Builds/Android";
    private const string ApkName = "RSAF_VRWarehouse.apk";
    private const string ReportFileName = "build-report.md";

    public static void Build()
    {
        try
        {
            var scenes = EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .ToArray();

            if (scenes.Length == 0)
            {
                throw new InvalidOperationException("No enabled scenes in build settings.");
            }

            Directory.CreateDirectory(OutputDirectory);
            var apkPath = Path.Combine(OutputDirectory, ApkName);

            Debug.Log($"[BuildQuestApk] building {scenes.Length} scene(s) -> {apkPath}");
            foreach (var scene in scenes)
            {
                Debug.Log($"[BuildQuestApk]   scene: {scene}");
            }

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = apkPath,
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = BuildOptions.None,
            };

            var report = BuildPipeline.BuildPlayer(options);
            WriteReport(report);

            if (report.summary.result != BuildResult.Succeeded)
            {
                Debug.LogError($"[BuildQuestApk] FAILED: {report.summary.result} " +
                               $"({report.summary.totalErrors} error(s))");
                EditorApplication.Exit(1);
                return;
            }

            Debug.Log($"[BuildQuestApk] OK — {report.summary.totalSize / (1024 * 1024)} MB " +
                      $"in {report.summary.totalTime}");
            EditorApplication.Exit(0);
        }
        catch (Exception e)
        {
            Debug.LogError($"[BuildQuestApk] EXCEPTION: {e}");
            EditorApplication.Exit(1);
        }
    }

    private static void WriteReport(BuildReport report)
    {
        var text = new StringBuilder();
        text.AppendLine("# Build report");
        text.AppendLine();
        text.AppendLine($"- Result: **{report.summary.result}**");
        text.AppendLine($"- Platform: {report.summary.platform}");
        text.AppendLine($"- Total size: **{report.summary.totalSize / (1024 * 1024)} MB**");
        text.AppendLine($"- Build time: {report.summary.totalTime}");
        text.AppendLine($"- Errors: {report.summary.totalErrors}, Warnings: {report.summary.totalWarnings}");
        text.AppendLine();

        try
        {
            var files = report.files;
            text.AppendLine("## Largest output files");
            text.AppendLine();
            foreach (var file in files.OrderByDescending(f => f.size).Take(25))
            {
                text.AppendLine($"- `{Path.GetFileName(file.path)}` — {file.size / (1024 * 1024)} MB ({file.role})");
            }
            text.AppendLine();
        }
        catch (Exception e)
        {
            text.AppendLine($"(file list unavailable: {e.Message})");
            text.AppendLine();
        }

        text.AppendLine("## Per-asset-type totals");
        text.AppendLine();
        try
        {
            foreach (var packedAssets in report.packedAssets)
            {
                var byType = packedAssets.contents
                    .GroupBy(c => c.type.Name)
                    .Select(g => new { Type = g.Key, Bytes = g.Sum(c => (long)c.packedSize) })
                    .OrderByDescending(g => g.Bytes)
                    .Take(12);

                text.AppendLine($"### `{packedAssets.shortPath}`");
                text.AppendLine();
                foreach (var entry in byType)
                {
                    text.AppendLine($"- {entry.Type}: {entry.Bytes / (1024 * 1024)} MB");
                }
                text.AppendLine();
            }
        }
        catch (Exception e)
        {
            text.AppendLine($"(packed asset breakdown unavailable: {e.Message})");
        }

        File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), ReportFileName), text.ToString());
        Debug.Log($"[BuildQuestApk] report written to {ReportFileName}");
    }
}
