using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Loads generated audio from Resources and caches it.
///
/// Clips live under Resources so they are guaranteed to survive IL2CPP
/// stripping and asset-bundling, which a plain scene reference is not.
/// Every lookup failure is reported once and then returns null, so a missing
/// clip degrades to silence rather than throwing every frame.
/// </summary>
public static class AudioLibrary
{
	public const string Root = "Audio";

	public const string MusicFolder = "Music";

	public const string AmbienceFolder = "Ambience";

	public const string SfxFolder = "SFX";

	private static readonly Dictionary<string, AudioClip> ClipCache =
		new Dictionary<string, AudioClip>();

	private static readonly Dictionary<string, AudioClip[]> VariantCache =
		new Dictionary<string, AudioClip[]>();

	private static readonly HashSet<string> ReportedMissing = new HashSet<string>();

	private static AudioMixer cachedMixer;

	/// <summary>The project mixer, or null if it could not be loaded.</summary>
	public static AudioMixer Mixer
	{
		get
		{
			if (cachedMixer == null)
			{
				cachedMixer = Resources.Load<AudioMixer>(Root + "/AudioMixer");
				if (cachedMixer == null)
				{
					ReportMissing(Root + "/AudioMixer", "audio mixer");
				}
			}
			return cachedMixer;
		}
	}

	/// <summary>Find a mixer group by name, or null if the mixer is unavailable.</summary>
	public static AudioMixerGroup Group(string channelName)
	{
		if (Mixer == null || string.IsNullOrEmpty(channelName))
		{
			return null;
		}
		AudioMixerGroup[] groups = Mixer.FindMatchingGroups(channelName);
		if (groups == null || groups.Length == 0)
		{
			ReportMissing(channelName, "mixer group");
			return null;
		}
		return groups[0];
	}

	/// <summary>Load one clip, e.g. Load(SfxFolder, "SFX_UI_Click").</summary>
	public static AudioClip Load(string folder, string clipName)
	{
		if (string.IsNullOrEmpty(clipName))
		{
			return null;
		}
		string key = Root + "/" + folder + "/" + clipName;
		if (ClipCache.TryGetValue(key, out AudioClip cached))
		{
			return cached;
		}

		AudioClip clip = Resources.Load<AudioClip>(key);
		if (clip == null)
		{
			ReportMissing(key, "audio clip");
		}
		ClipCache[key] = clip;
		return clip;
	}

	/// <summary>Load every clip in a folder whose name starts with a prefix.</summary>
	public static AudioClip[] LoadVariants(string folder, string prefix)
	{
		string key = folder + "/" + prefix;
		if (VariantCache.TryGetValue(key, out AudioClip[] cached))
		{
			return cached;
		}

		AudioClip[] all = Resources.LoadAll<AudioClip>(Root + "/" + folder);
		List<AudioClip> matches = new List<AudioClip>();
		if (all != null)
		{
			for (int i = 0; i < all.Length; i++)
			{
				if (all[i] != null && all[i].name.StartsWith(prefix))
				{
					matches.Add(all[i]);
				}
			}
		}
		if (matches.Count == 0)
		{
			ReportMissing(key, "clip variants");
		}

		AudioClip[] result = matches.ToArray();
		VariantCache[key] = result;
		return result;
	}

	private static void ReportMissing(string key, string kind)
	{
		if (ReportedMissing.Add(key))
		{
			Debug.LogWarning("[Audio] Missing " + kind + " at Resources/" + key +
				". That sound will be silent.");
		}
	}
}
