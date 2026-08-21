using UnityEngine;

/// <summary>
/// A non-positional looping ambience bed — the constant air of the room.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class AmbienceBed : MonoBehaviour
{
	[Tooltip("Clip name inside Resources/Audio/Ambience")]
	public string clipName;

	[Range(0f, 1f)]
	public float volume = 0.6f;

	private AudioSource source;

	private void Awake()
	{
		source = GetComponent<AudioSource>();
		source.playOnAwake = false;
		source.loop = true;
		source.spatialBlend = 0f;
		source.outputAudioMixerGroup = AudioLibrary.Group(AudioChannels.Ambience);
	}

	private void OnEnable()
	{
		AudioClip clip = AudioLibrary.Load(AudioLibrary.AmbienceFolder, clipName);
		if (clip == null)
		{
			return;
		}
		source.clip = clip;
		source.volume = volume;
		// Start at a random offset so reloading a scene does not replay the
		// same few seconds every time.
		source.time = Random.Range(0f, Mathf.Max(clip.length - 0.1f, 0f));
		source.Play();
	}

	private void OnDisable()
	{
		if (source != null)
		{
			source.Stop();
		}
	}
}
