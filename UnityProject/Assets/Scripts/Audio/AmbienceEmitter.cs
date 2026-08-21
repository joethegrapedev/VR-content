using UnityEngine;

/// <summary>
/// A positional looping ambience source — vents, lights, machinery.
///
/// Uses logarithmic rolloff so sound falls away with distance the way it does
/// in a real space, which is what gives a VR scene its sense of size.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class AmbienceEmitter : MonoBehaviour
{
	[Tooltip("Clip name inside Resources/Audio/Ambience")]
	public string clipName;

	[Range(0f, 1f)]
	public float volume = 0.5f;

	[Tooltip("Distance at which the sound is at full volume")]
	public float minDistance = 1.5f;

	[Tooltip("Distance beyond which the sound is inaudible")]
	public float maxDistance = 14f;

	private AudioSource source;

	private void Awake()
	{
		source = GetComponent<AudioSource>();
		source.playOnAwake = false;
		source.loop = true;
		source.spatialBlend = 1f;
		source.rolloffMode = AudioRolloffMode.Logarithmic;
		source.minDistance = minDistance;
		source.maxDistance = maxDistance;
		source.dopplerLevel = 0f;
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
		// Decorrelate emitters so several copies of one loop do not phase
		// together into an obvious pulse.
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
