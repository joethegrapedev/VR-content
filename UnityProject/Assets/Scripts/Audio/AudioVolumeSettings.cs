using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// The player's saved mixer volumes.
///
/// Immutable: changing a channel returns a new instance rather than editing
/// this one. That is what fixes the original bug, where SaveSettings kept
/// appending to a single shared list so the second save silently wrote stale
/// values at indices 0 and 1.
///
/// Files written by the original build hold only two entries (dialogue, SFX);
/// those load fine and pick up defaults for the two new channels.
/// </summary>
public class AudioVolumeSettings
{
	public const int DialogueIndex = 0;

	public const int SfxIndex = 1;

	public const int MusicIndex = 2;

	public const int AmbienceIndex = 3;

	public readonly float Dialogue;

	public readonly float Sfx;

	public readonly float Music;

	public readonly float Ambience;

	public AudioVolumeSettings(float dialogue, float sfx, float music, float ambience)
	{
		Dialogue = Mathf.Clamp01(dialogue);
		Sfx = Mathf.Clamp01(sfx);
		Music = Mathf.Clamp01(music);
		Ambience = Mathf.Clamp01(ambience);
	}

	public static AudioVolumeSettings Defaults =>
		new AudioVolumeSettings(AudioChannels.DefaultDialogue, AudioChannels.DefaultSfx,
			AudioChannels.DefaultMusic, AudioChannels.DefaultAmbience);

	public AudioVolumeSettings WithDialogue(float value) =>
		new AudioVolumeSettings(value, Sfx, Music, Ambience);

	public AudioVolumeSettings WithSfx(float value) =>
		new AudioVolumeSettings(Dialogue, value, Music, Ambience);

	public AudioVolumeSettings WithMusic(float value) =>
		new AudioVolumeSettings(Dialogue, Sfx, value, Ambience);

	public AudioVolumeSettings WithAmbience(float value) =>
		new AudioVolumeSettings(Dialogue, Sfx, Music, value);

	/// <summary>Read the saved settings, falling back to defaults on any problem.</summary>
	public static AudioVolumeSettings Load()
	{
		string path = SavePaths.AudioSettingsFile;
		try
		{
			if (!File.Exists(path))
			{
				return Defaults;
			}

			Audio stored = JsonUtility.FromJson<Audio>(File.ReadAllText(path));
			if (stored == null || stored.Volume == null || stored.Volume.Count == 0)
			{
				return Defaults;
			}

			AudioVolumeSettings defaults = Defaults;
			return new AudioVolumeSettings(
				ValueAt(stored.Volume, DialogueIndex, defaults.Dialogue),
				ValueAt(stored.Volume, SfxIndex, defaults.Sfx),
				ValueAt(stored.Volume, MusicIndex, defaults.Music),
				ValueAt(stored.Volume, AmbienceIndex, defaults.Ambience));
		}
		catch (Exception error)
		{
			Debug.LogWarning("[Audio] Could not read volume settings from " + path +
				" (" + error.Message + "). Using defaults.");
			return Defaults;
		}
	}

	/// <summary>Persist these settings. Returns false and logs on failure.</summary>
	public bool Save()
	{
		Audio payload = new Audio();
		payload.Volume = new List<float> { Dialogue, Sfx, Music, Ambience };

		string contents = JsonUtility.ToJson(payload, prettyPrint: true);
		if (SavePaths.TryWriteAllText(SavePaths.AudioSettingsFile, contents,
			out string error))
		{
			return true;
		}

		Debug.LogError("[Audio] Could not save volume settings: " + error);
		return false;
	}

	/// <summary>Apply every channel to the mixer.</summary>
	public void ApplyTo(AudioMixer mixer)
	{
		if (mixer == null)
		{
			return;
		}
		SetChannel(mixer, AudioChannels.Dialogue, Dialogue);
		SetChannel(mixer, AudioChannels.Sfx, Sfx);
		SetChannel(mixer, AudioChannels.Music, Music);
		SetChannel(mixer, AudioChannels.Ambience, Ambience);
	}

	private static void SetChannel(AudioMixer mixer, string channel, float linear)
	{
		// SetFloat returns false when the parameter is not exposed; that was
		// silently the case for every channel before the mixer was repaired.
		if (!mixer.SetFloat(channel, AudioChannels.ToDecibels(linear)))
		{
			Debug.LogWarning("[Audio] Mixer parameter '" + channel +
				"' is not exposed; that channel's volume slider will do nothing.");
		}
	}

	private static float ValueAt(List<float> values, int index, float fallback)
	{
		return (index >= 0 && index < values.Count) ? values[index] : fallback;
	}
}
