using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Single source of truth for the game's on-disk locations.
///
/// The original build wrote everything under <see cref="Application.dataPath"/>, which is
/// writable on a Windows standalone player (it is the *_Data folder next to the .exe). On
/// Android/Quest that same property resolves inside the read-only APK, so every write throws
/// and aborts its caller — which is what stopped the app from ever reaching the "5SD Map"
/// scene after name entry.
///
/// All WRITES therefore go to <see cref="Application.persistentDataPath"/>, which is writable
/// on every platform. READS prefer the persistent copy and fall back to any copy that shipped
/// inside the build, so data authored at build time is still found.
/// </summary>
public static class SavePaths
{
    private const string SavesFolderName = "_MyProject/saves";
    private const string OutputFolderName = "Output";
    private const string PlayerInfoFileName = "PlayerInfo.json";
    private const string AudioSettingsFileName = "AudioSettings.json";

    /// <summary>Writable directory holding player save JSON.</summary>
    public static string SavesDirectory => Path.Combine(Application.persistentDataPath, SavesFolderName);

    /// <summary>Writable directory holding generated leaderboard spreadsheets.</summary>
    public static string OutputDirectory => Path.Combine(Application.persistentDataPath, OutputFolderName);

    public static string PlayerInfoFile => Path.Combine(SavesDirectory, PlayerInfoFileName);

    public static string AudioSettingsFile => Path.Combine(SavesDirectory, AudioSettingsFileName);

    /// <summary>Read-only directory of content that shipped inside the build.</summary>
    private static string ShippedRoot => Application.dataPath;

    /// <summary>
    /// Creates <paramref name="directory"/> if missing. Returns false and reports the reason
    /// instead of throwing, so a caller can degrade gracefully rather than abort mid-flow.
    /// </summary>
    public static bool TryEnsureDirectory(string directory, out string error)
    {
        error = null;
        if (string.IsNullOrEmpty(directory))
        {
            error = "Directory path was null or empty.";
            return false;
        }

        try
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            return true;
        }
        catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is NotSupportedException)
        {
            error = $"Could not create directory '{directory}': {e.Message}";
            Debug.LogError($"[SavePaths] {error}");
            return false;
        }
    }

    /// <summary>
    /// Writes <paramref name="contents"/> to <paramref name="filePath"/>, creating the parent
    /// directory first. Returns false on failure rather than throwing.
    /// </summary>
    public static bool TryWriteAllText(string filePath, string contents, out string error)
    {
        error = null;
        if (string.IsNullOrEmpty(filePath))
        {
            error = "File path was null or empty.";
            return false;
        }

        var directory = Path.GetDirectoryName(filePath);
        if (!TryEnsureDirectory(directory, out error))
        {
            return false;
        }

        try
        {
            File.WriteAllText(filePath, contents);
            return true;
        }
        catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
        {
            error = $"Could not write '{filePath}': {e.Message}";
            Debug.LogError($"[SavePaths] {error}");
            return false;
        }
    }

    /// <summary>
    /// Reads text, preferring the writable persistent copy and falling back to any copy that
    /// shipped in the build. Returns false if neither exists or the read fails.
    /// </summary>
    public static bool TryReadAllText(string persistentFilePath, string shippedRelativePath, out string contents, out string error)
    {
        contents = null;
        error = null;

        try
        {
            if (!string.IsNullOrEmpty(persistentFilePath) && File.Exists(persistentFilePath))
            {
                contents = File.ReadAllText(persistentFilePath);
                return true;
            }

            if (!string.IsNullOrEmpty(shippedRelativePath))
            {
                var shipped = Path.Combine(ShippedRoot, shippedRelativePath);
                if (File.Exists(shipped))
                {
                    contents = File.ReadAllText(shipped);
                    return true;
                }
            }

            error = $"No readable file at '{persistentFilePath}'.";
            return false;
        }
        catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
        {
            error = $"Could not read '{persistentFilePath}': {e.Message}";
            Debug.LogError($"[SavePaths] {error}");
            return false;
        }
    }
}
