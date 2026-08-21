using UnityEngine;

/// <summary>
/// Gives every physical VR button a press sound.
///
/// The recovered Keyboard scene has 132 ButtonVR keys but only 67 of them
/// carry a clip, so half the keyboard was silent. ButtonVR caches its
/// AudioSource in Start and calls Play() unconditionally, so this must run in
/// Awake — both to fill the empty clip slots and to make sure the component
/// it caches actually exists.
/// </summary>
public static class ButtonVrSoundBinder
{
	private const string KeyClip = "SFX_Keyboard_Key";

	private const float KeyVolume = 0.55f;

	/// <summary>Ensure every ButtonVR in the scene has a working sound source.</summary>
	/// <returns>How many buttons were repaired.</returns>
	public static int BindAll()
	{
		AudioClip clip = AudioLibrary.Load(AudioLibrary.SfxFolder, KeyClip);
		if (clip == null)
		{
			return 0;
		}

		ButtonVR[] buttons = Object.FindObjectsOfType<ButtonVR>(true);
		AudioMixerGroupCache group = new AudioMixerGroupCache();
		int repaired = 0;

		for (int i = 0; i < buttons.Length; i++)
		{
			if (Bind(buttons[i], clip, group))
			{
				repaired++;
			}
		}

		if (repaired > 0)
		{
			Debug.Log("[Audio] Gave " + repaired + " of " + buttons.Length +
				" VR buttons a press sound.");
		}
		return repaired;
	}

	private static bool Bind(ButtonVR button, AudioClip clip,
		AudioMixerGroupCache group)
	{
		if (button == null)
		{
			return false;
		}

		AudioSource source = button.GetComponent<AudioSource>();
		bool repaired = false;

		if (source == null)
		{
			source = button.gameObject.AddComponent<AudioSource>();
			repaired = true;
		}
		if (source.clip == null)
		{
			source.clip = clip;
			source.volume = KeyVolume;
			repaired = true;
		}

		source.playOnAwake = false;
		// Keys are objects in the room, so they should sound like it.
		source.spatialBlend = 1f;
		source.rolloffMode = AudioRolloffMode.Logarithmic;
		source.minDistance = 0.4f;
		source.maxDistance = 6f;
		source.dopplerLevel = 0f;
		source.outputAudioMixerGroup = group.SfxGroup;
		return repaired;
	}

	/// <summary>Avoids a mixer lookup per button across 132 keys.</summary>
	private class AudioMixerGroupCache
	{
		private UnityEngine.Audio.AudioMixerGroup sfx;

		private bool resolved;

		public UnityEngine.Audio.AudioMixerGroup SfxGroup
		{
			get
			{
				if (!resolved)
				{
					sfx = AudioLibrary.Group(AudioChannels.Sfx);
					resolved = true;
				}
				return sfx;
			}
		}
	}
}
