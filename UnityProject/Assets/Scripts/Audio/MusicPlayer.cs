using System.Collections;
using UnityEngine;

/// <summary>
/// Persistent background music with crossfades between tracks.
///
/// Survives scene loads so the menu theme does not restart when the player
/// moves between Menu, Keyboard and Leaderboard. Requesting the track that is
/// already playing is a no-op, which is what keeps it continuous.
/// </summary>
public class MusicPlayer : MonoBehaviour
{
	public const float DefaultFadeSeconds = 1.5f;

	private static MusicPlayer instance;

	private AudioSource sourceA;

	private AudioSource sourceB;

	private AudioSource activeSource;

	private string currentTrack = string.Empty;

	private Coroutine fadeRoutine;

	private float trackVolume = 1f;

	/// <summary>The single player instance, created on first use.</summary>
	public static MusicPlayer Instance
	{
		get
		{
			if (instance == null)
			{
				GameObject host = new GameObject("[MusicPlayer]");
				instance = host.AddComponent<MusicPlayer>();
				Object.DontDestroyOnLoad(host);
			}
			return instance;
		}
	}

	public string CurrentTrack => currentTrack;

	private void Awake()
	{
		if (instance != null && instance != this)
		{
			Object.Destroy(gameObject);
			return;
		}
		instance = this;
		Object.DontDestroyOnLoad(gameObject);
		sourceA = CreateSource("Music A");
		sourceB = CreateSource("Music B");
		activeSource = sourceA;
	}

	private AudioSource CreateSource(string sourceName)
	{
		GameObject child = new GameObject(sourceName);
		child.transform.SetParent(transform, worldPositionStays: false);
		AudioSource source = child.AddComponent<AudioSource>();
		source.playOnAwake = false;
		source.loop = true;
		source.volume = 0f;
		// Music is non-diegetic: fully 2-D so it does not pan with head turns.
		source.spatialBlend = 0f;
		source.outputAudioMixerGroup = AudioLibrary.Group(AudioChannels.Music);
		return source;
	}

	/// <summary>Crossfade to a track in Resources/Audio/Music, or stop if null.</summary>
	public void Play(string trackName, float volume = 1f,
		float fadeSeconds = DefaultFadeSeconds)
	{
		if (string.IsNullOrEmpty(trackName))
		{
			Stop(fadeSeconds);
			return;
		}
		if (trackName == currentTrack && activeSource != null && activeSource.isPlaying)
		{
			trackVolume = volume;
			return;
		}

		AudioClip clip = AudioLibrary.Load(AudioLibrary.MusicFolder, trackName);
		if (clip == null)
		{
			return;
		}

		AudioSource incoming = (activeSource == sourceA) ? sourceB : sourceA;
		AudioSource outgoing = activeSource;

		incoming.clip = clip;
		incoming.volume = 0f;
		incoming.time = 0f;
		incoming.Play();

		currentTrack = trackName;
		trackVolume = volume;
		activeSource = incoming;
		RestartFade(Crossfade(incoming, outgoing, volume, fadeSeconds));
	}

	/// <summary>Fade the current track out and stop it.</summary>
	public void Stop(float fadeSeconds = DefaultFadeSeconds)
	{
		if (string.IsNullOrEmpty(currentTrack))
		{
			return;
		}
		currentTrack = string.Empty;
		RestartFade(Crossfade(null, activeSource, 0f, fadeSeconds));
	}

	/// <summary>Scale the active track without changing which track plays.</summary>
	public void SetVolume(float volume)
	{
		trackVolume = Mathf.Clamp01(volume);
		if (fadeRoutine == null && activeSource != null)
		{
			activeSource.volume = trackVolume;
		}
	}

	private void RestartFade(IEnumerator routine)
	{
		if (fadeRoutine != null)
		{
			StopCoroutine(fadeRoutine);
		}
		fadeRoutine = StartCoroutine(routine);
	}

	private IEnumerator Crossfade(AudioSource incoming, AudioSource outgoing,
		float targetVolume, float fadeSeconds)
	{
		float startIncoming = (incoming != null) ? incoming.volume : 0f;
		float startOutgoing = (outgoing != null) ? outgoing.volume : 0f;
		float elapsed = 0f;

		while (elapsed < fadeSeconds)
		{
			// Unscaled: the pause menu sets timeScale to 0 and music must still fade.
			elapsed += Time.unscaledDeltaTime;
			float progress = Mathf.Clamp01(elapsed / Mathf.Max(fadeSeconds, 0.0001f));
			if (incoming != null)
			{
				incoming.volume = Mathf.Lerp(startIncoming, targetVolume, progress);
			}
			if (outgoing != null && outgoing != incoming)
			{
				outgoing.volume = Mathf.Lerp(startOutgoing, 0f, progress);
			}
			yield return null;
		}

		if (incoming != null)
		{
			incoming.volume = targetVolume;
		}
		if (outgoing != null && outgoing != incoming)
		{
			outgoing.volume = 0f;
			outgoing.Stop();
			outgoing.clip = null;
		}
		fadeRoutine = null;
	}
}
