using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

/// <summary>
/// Headless checks that the audio wiring actually holds, so a broken setup
/// fails the build rather than shipping as silence.
/// </summary>
public static class ValidateAudio
{
	private static readonly string[] RequiredChannels =
	{
		AudioChannels.Master, AudioChannels.Dialogue, AudioChannels.Sfx,
		AudioChannels.Music, AudioChannels.Ambience
	};

	private static readonly string[] RequiredClips =
	{
		"Audio/Music/Music_Menu",
		"Audio/Music/Music_Gameplay_Calm",
		"Audio/Ambience/Amb_Warehouse_RoomTone",
		"Audio/SFX/SFX_UI_Click",
		"Audio/SFX/SFX_Footstep_Concrete_01"
	};

	[MenuItem("Rebuild/Validate Audio")]
	public static void Run()
	{
		List<string> failures = new List<string>();

		AudioMixer mixer = Resources.Load<AudioMixer>("Audio/AudioMixer");
		if (mixer == null)
		{
			failures.Add("AudioMixer not loadable from Resources/Audio/AudioMixer");
		}
		else
		{
			CheckChannels(mixer, failures);
		}

		CheckClips(failures);
		CheckScenes(failures);
		Report(failures);
	}

	private static void CheckChannels(AudioMixer mixer, List<string> failures)
	{
		HashSet<string> exposed = ExposedParameterNames(mixer);
		foreach (string channel in RequiredChannels)
		{
			AudioMixerGroup[] groups = mixer.FindMatchingGroups(channel);
			if (groups == null || groups.Length == 0)
			{
				failures.Add($"Mixer group missing: {channel}");
			}
			// An exposed parameter is the only way the volume sliders reach the
			// mixer. SetFloat cannot be used to test this: outside play mode it
			// always returns false, so it would report every channel as broken.
			if (!exposed.Contains(channel))
			{
				failures.Add($"Mixer parameter not exposed: {channel}");
			}
		}
	}

	/// <summary>Read the exposed parameter names straight from the asset.</summary>
	private static HashSet<string> ExposedParameterNames(AudioMixer mixer)
	{
		HashSet<string> names = new HashSet<string>();
		SerializedObject serialized = new SerializedObject(mixer);
		SerializedProperty parameters = serialized.FindProperty("m_ExposedParameters");
		if (parameters == null || !parameters.isArray)
		{
			return names;
		}

		for (int i = 0; i < parameters.arraySize; i++)
		{
			SerializedProperty entry = parameters.GetArrayElementAtIndex(i);
			SerializedProperty name = entry.FindPropertyRelative("name");
			if (name != null && !string.IsNullOrEmpty(name.stringValue))
			{
				names.Add(name.stringValue);
			}
		}
		return names;
	}

	private static void CheckClips(List<string> failures)
	{
		foreach (string path in RequiredClips)
		{
			if (Resources.Load<AudioClip>(path) == null)
			{
				failures.Add($"Clip missing from Resources: {path}");
			}
		}
	}

	private static void CheckScenes(List<string> failures)
	{
		foreach (EditorBuildSettingsScene entry in EditorBuildSettings.scenes)
		{
			if (!entry.enabled)
			{
				continue;
			}

			Scene scene = EditorSceneManager.OpenScene(entry.path, OpenSceneMode.Single);
			bool found = false;
			foreach (GameObject root in scene.GetRootGameObjects())
			{
				if (root.GetComponentInChildren<SceneAudioDirector>(true) != null)
				{
					found = true;
					break;
				}
			}
			if (!found)
			{
				failures.Add($"Scene has no SceneAudioDirector: {scene.name}");
			}
		}
	}

	private static void Report(List<string> failures)
	{
		if (failures.Count == 0)
		{
			Debug.Log("[ValidateAudio] PASS - all audio checks succeeded.");
			return;
		}

		StringBuilder message = new StringBuilder("[ValidateAudio] FAIL\n");
		foreach (string failure in failures)
		{
			message.AppendLine("  - " + failure);
		}
		Debug.LogError(message.ToString());
		if (Application.isBatchMode)
		{
			EditorApplication.Exit(1);
		}
	}
}
