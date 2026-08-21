using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Applies import settings to the generated audio and drops a
/// <see cref="SceneAudioDirector"/> into every gameplay scene.
///
/// The generated clips are shipped as lossless WAV masters; Unity compresses
/// them to Vorbis for the Android player here, so nothing is encoded twice.
/// </summary>
public static class ConfigureAudio
{
	private const string GeneratedRoot = "Assets/Resources/Audio";

	private const string DirectorObjectName = "[SceneAudio]";

	/// <summary>Long clips stream from disk instead of sitting in memory.</summary>
	private const float StreamingThresholdSeconds = 20f;

	private static readonly string[] TargetScenes =
	{
		"Assets/_MyProject/Scenes/Menu.unity",
		"Assets/_MyProject/Scenes/Keyboard.unity",
		"Assets/_MyProject/Scenes/Leaderboard.unity",
		"Assets/_MyProject/Scenes/Tutorial.unity",
		"Assets/_MyProject/Scenes/5SD Map.unity",
		"Assets/_MyProject/Scenes/Audio Settings.unity"
	};

	[MenuItem("Rebuild/Configure Audio")]
	public static void Run()
	{
		int configured = ConfigureImporters();
		int wired = WireScenes();
		Debug.Log($"[ConfigureAudio] Configured {configured} clips, wired {wired} scenes.");
	}

	/// <summary>Set compression and load behaviour on every generated clip.</summary>
	public static int ConfigureImporters()
	{
		string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { GeneratedRoot });
		int changed = 0;

		foreach (string guid in guids)
		{
			string path = AssetDatabase.GUIDToAssetPath(guid);
			AudioImporter importer = AssetImporter.GetAtPath(path) as AudioImporter;
			if (importer == null)
			{
				Debug.LogWarning($"[ConfigureAudio] No AudioImporter for {path}");
				continue;
			}

			AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
			bool longClip = clip != null && clip.length >= StreamingThresholdSeconds;

			AudioImporterSampleSettings settings = importer.defaultSampleSettings;
			settings.compressionFormat = AudioCompressionFormat.Vorbis;
			settings.quality = longClip ? 0.55f : 0.7f;
			settings.loadType = longClip
				? AudioClipLoadType.Streaming
				: AudioClipLoadType.CompressedInMemory;

			importer.defaultSampleSettings = settings;
			importer.loadInBackground = longClip;
			importer.preloadAudioData = !longClip;
			// Spatialised sources must stay mono; music and beds are already stereo.
			importer.forceToMono = false;

			EditorUtility.SetDirty(importer);
			importer.SaveAndReimport();
			changed++;
		}

		AssetDatabase.SaveAssets();
		return changed;
	}

	/// <summary>Ensure each scene contains exactly one audio director.</summary>
	public static int WireScenes()
	{
		int wired = 0;
		foreach (string scenePath in TargetScenes)
		{
			if (!System.IO.File.Exists(scenePath))
			{
				Debug.LogWarning($"[ConfigureAudio] Scene not found: {scenePath}");
				continue;
			}

			Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
			if (EnsureDirector(scene))
			{
				EditorSceneManager.MarkSceneDirty(scene);
				EditorSceneManager.SaveScene(scene);
				wired++;
				Debug.Log($"[ConfigureAudio] Added audio director to {scene.name}");
			}
			else
			{
				Debug.Log($"[ConfigureAudio] {scene.name} already had an audio director");
			}
		}
		return wired;
	}

	private static bool EnsureDirector(Scene scene)
	{
		foreach (GameObject root in scene.GetRootGameObjects())
		{
			if (root.GetComponentInChildren<SceneAudioDirector>(true) != null)
			{
				return false;
			}
		}

		GameObject host = new GameObject(DirectorObjectName);
		host.AddComponent<SceneAudioDirector>();
		SceneManager.MoveGameObjectToScene(host, scene);
		return true;
	}
}
