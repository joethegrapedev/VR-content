using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

/// <summary>
/// Sets up all audio for whichever scene it lives in: music, the ambience
/// bed, positional emitters, footsteps and UI sounds.
///
/// One of these per scene replaces hand-wiring hundreds of AudioSources, and
/// it re-applies the player's saved volumes in every scene — the original
/// AudioSettings only did so in Menu and 5SD Map.
/// </summary>
public class SceneAudioDirector : MonoBehaviour
{
	[Tooltip("Spawn looping emitters on named scene objects (vents, fans, forklifts)")]
	public bool spawnEmitters = true;

	[Tooltip("Bind hover and click sounds to buttons in this scene")]
	public bool bindUiSounds = true;

	private readonly List<GameObject> spawned = new List<GameObject>();

	private void Awake()
	{
		// Must run before ButtonVR.Start caches its AudioSource, otherwise the
		// keys it repairs would be ignored for the rest of the scene's life.
		ButtonVrSoundBinder.BindAll();
	}

	private void Start()
	{
		string sceneName = SceneManager.GetActiveScene().name;
		SceneAudioProfile profile = SceneAudioProfiles.For(sceneName);

		ApplySavedVolumes();
		StartMusic(profile);
		StartAmbienceBed(profile);

		if (profile.Footsteps)
		{
			EnsureComponent<FootstepPlayer>();
		}
		if (bindUiSounds)
		{
			EnsureComponent<UiSoundBinder>();
		}
		if (spawnEmitters && profile.PositionalEmitters)
		{
			SpawnEmitters(AmbienceEmitterRules.Warehouse);
		}
		if (profile.PositionalEmitters)
		{
			AttachImpactSounds();
		}
	}

	private void OnDestroy()
	{
		for (int i = 0; i < spawned.Count; i++)
		{
			if (spawned[i] != null)
			{
				Object.Destroy(spawned[i]);
			}
		}
		spawned.Clear();
	}

	/// <summary>Push the saved mixer volumes, so every scene honours the settings.</summary>
	private void ApplySavedVolumes()
	{
		AudioMixer mixer = AudioLibrary.Mixer;
		if (mixer == null)
		{
			return;
		}
		AudioVolumeSettings settings = AudioVolumeSettings.Load();
		settings.ApplyTo(mixer);
	}

	private void StartMusic(SceneAudioProfile profile)
	{
		if (string.IsNullOrEmpty(profile.MusicTrack))
		{
			MusicPlayer.Instance.Stop();
			return;
		}
		MusicPlayer.Instance.Play(profile.MusicTrack, profile.MusicVolume);
	}

	private void StartAmbienceBed(SceneAudioProfile profile)
	{
		if (string.IsNullOrEmpty(profile.AmbienceBed))
		{
			return;
		}

		GameObject host = new GameObject("[AmbienceBed]");
		host.transform.SetParent(transform, worldPositionStays: false);
		AmbienceBed bed = host.AddComponent<AmbienceBed>();
		bed.clipName = profile.AmbienceBed;
		bed.volume = profile.AmbienceVolume;
		bed.enabled = true;
		spawned.Add(host);
	}

	/// <summary>Walk the scene once and hang emitters off keyword-matched objects.</summary>
	private void SpawnEmitters(AmbienceEmitterRule[] rules)
	{
		if (rules == null || rules.Length == 0)
		{
			return;
		}

		Transform[] all = Object.FindObjectsOfType<Transform>();
		int[] placed = new int[rules.Length];

		for (int i = 0; i < all.Length; i++)
		{
			Transform candidate = all[i];
			if (candidate == null)
			{
				continue;
			}
			string lowered = candidate.name.ToLowerInvariant();

			for (int r = 0; r < rules.Length; r++)
			{
				AmbienceEmitterRule rule = rules[r];
				if (placed[r] >= rule.MaxCount || !lowered.Contains(rule.Keyword))
				{
					continue;
				}
				AttachEmitter(candidate, rule);
				placed[r]++;
				break;
			}
		}

		LogPlacement(rules, placed);
	}

	private void AttachEmitter(Transform anchor, AmbienceEmitterRule rule)
	{
		GameObject host = new GameObject("[Ambience] " + rule.ClipName);
		host.transform.SetParent(anchor, worldPositionStays: false);
		host.transform.localPosition = Vector3.zero;

		AmbienceEmitter emitter = host.AddComponent<AmbienceEmitter>();
		emitter.clipName = rule.ClipName;
		emitter.volume = rule.Volume;
		emitter.maxDistance = rule.MaxDistance;
		spawned.Add(host);
	}

	private void LogPlacement(AmbienceEmitterRule[] rules, int[] placed)
	{
		int total = 0;
		for (int i = 0; i < placed.Length; i++)
		{
			total += placed[i];
		}
		if (total == 0)
		{
			Debug.LogWarning("[Audio] No ambience emitters matched this scene. " +
				"Positional ambience will be absent.");
			return;
		}
		Debug.Log("[Audio] Placed " + total + " ambience emitters.");
	}

	/// <summary>Give loose physics props an impact sound.</summary>
	private void AttachImpactSounds()
	{
		Rigidbody[] bodies = Object.FindObjectsOfType<Rigidbody>();
		int attached = 0;
		for (int i = 0; i < bodies.Length; i++)
		{
			Rigidbody body = bodies[i];
			// Kinematic bodies are scenery, not things that can be dropped.
			if (body == null || body.isKinematic ||
				body.GetComponent<ImpactSound>() != null)
			{
				continue;
			}
			body.gameObject.AddComponent<ImpactSound>();
			attached++;
		}
		if (attached > 0)
		{
			Debug.Log("[Audio] Attached impact sounds to " + attached + " objects.");
		}
	}

	private T EnsureComponent<T>() where T : Component
	{
		T existing = GetComponent<T>();
		return (existing != null) ? existing : gameObject.AddComponent<T>();
	}
}
