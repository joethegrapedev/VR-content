using UnityEngine;

/// <summary>
/// Fire-and-forget one-shot playback routed through the SFX mixer group.
/// </summary>
public static class SfxPlayer
{
	private static AudioSource uiSource;

	/// <summary>Play a non-positional one-shot (UI, notifications).</summary>
	public static void PlayUi(string clipName, float volume = 1f)
	{
		AudioClip clip = AudioLibrary.Load(AudioLibrary.SfxFolder, clipName);
		if (clip == null)
		{
			return;
		}
		EnsureUiSource();
		if (uiSource != null)
		{
			uiSource.PlayOneShot(clip, Mathf.Clamp01(volume));
		}
	}

	/// <summary>Play a one-shot at a world position with 3-D falloff.</summary>
	public static void PlayAt(string clipName, Vector3 position, float volume = 1f)
	{
		AudioClip clip = AudioLibrary.Load(AudioLibrary.SfxFolder, clipName);
		if (clip == null)
		{
			return;
		}

		GameObject host = new GameObject("OneShot_" + clipName);
		host.transform.position = position;
		AudioSource source = host.AddComponent<AudioSource>();
		source.clip = clip;
		source.volume = Mathf.Clamp01(volume);
		source.spatialBlend = 1f;
		source.rolloffMode = AudioRolloffMode.Logarithmic;
		source.minDistance = 1.5f;
		source.maxDistance = 22f;
		source.dopplerLevel = 0f;
		source.outputAudioMixerGroup = AudioLibrary.Group(AudioChannels.Sfx);
		source.Play();
		Object.Destroy(host, clip.length + 0.2f);
	}

	/// <summary>Play a random clip from a numbered variant set.</summary>
	public static void PlayVariantAt(string prefix, Vector3 position, float volume = 1f,
		float minPitch = 0.94f, float maxPitch = 1.06f)
	{
		AudioClip[] variants = AudioLibrary.LoadVariants(AudioLibrary.SfxFolder, prefix);
		if (variants == null || variants.Length == 0)
		{
			return;
		}

		AudioClip clip = variants[Random.Range(0, variants.Length)];
		GameObject host = new GameObject("OneShot_" + clip.name);
		host.transform.position = position;
		AudioSource source = host.AddComponent<AudioSource>();
		source.clip = clip;
		source.volume = Mathf.Clamp01(volume);
		// Pitch variation stops repeated footsteps sounding like a machine gun.
		source.pitch = Random.Range(minPitch, maxPitch);
		source.spatialBlend = 1f;
		source.rolloffMode = AudioRolloffMode.Logarithmic;
		source.minDistance = 1f;
		source.maxDistance = 16f;
		source.dopplerLevel = 0f;
		source.outputAudioMixerGroup = AudioLibrary.Group(AudioChannels.Sfx);
		source.Play();
		Object.Destroy(host, clip.length / Mathf.Max(source.pitch, 0.01f) + 0.2f);
	}

	private static void EnsureUiSource()
	{
		if (uiSource != null)
		{
			return;
		}
		GameObject host = new GameObject("[UiSfx]");
		Object.DontDestroyOnLoad(host);
		uiSource = host.AddComponent<AudioSource>();
		uiSource.playOnAwake = false;
		uiSource.spatialBlend = 0f;
		uiSource.outputAudioMixerGroup = AudioLibrary.Group(AudioChannels.Sfx);
	}
}
