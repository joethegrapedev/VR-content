using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

/// <summary>
/// Drives the volume sliders in the Audio Settings scene and applies the
/// saved mixer levels.
///
/// Three defects from the recovered original are fixed here:
///   1. Volumes were only applied in Menu and 5SD Map, so Tutorial, Keyboard
///      and Leaderboard ignored the player's settings entirely.
///   2. SaveSettings appended to a shared list on every save, so the second
///      save wrote stale values at the indices the loader reads.
///   3. The mixer parameters were not exposed under these names at all, so
///      every SetFloat call silently did nothing.
///
/// The serialized field names are unchanged so existing scene bindings still
/// resolve.
/// </summary>
public class AudioSettings : MonoBehaviour
{
	[SerializeField]
	private AudioMixer myAudioMixer;

	public Slider Dialogue;

	public Slider SFX;

	[Tooltip("Optional — only present in the Audio Settings scene")]
	public Slider Music;

	[Tooltip("Optional — only present in the Audio Settings scene")]
	public Slider Ambience;

	private AudioVolumeSettings settings = AudioVolumeSettings.Defaults;

	private AudioMixer Mixer =>
		(myAudioMixer != null) ? myAudioMixer : AudioLibrary.Mixer;

	protected void Start()
	{
		settings = AudioVolumeSettings.Load();
		settings.ApplyTo(Mixer);
		PushToSliders();
	}

	/// <summary>Mirror the loaded values onto whichever sliders exist.</summary>
	private void PushToSliders()
	{
		SetSlider(Dialogue, settings.Dialogue);
		SetSlider(SFX, settings.Sfx);
		SetSlider(Music, settings.Music);
		SetSlider(Ambience, settings.Ambience);
	}

	private static void SetSlider(Slider slider, float value)
	{
		if (slider != null)
		{
			slider.SetValueWithoutNotify(value);
		}
	}

	public void UpdateDialogue(float sliderValue)
	{
		settings = settings.WithDialogue(sliderValue);
		settings.ApplyTo(Mixer);
	}

	public void UpdateSFX(float sliderValue)
	{
		settings = settings.WithSfx(sliderValue);
		settings.ApplyTo(Mixer);
	}

	public void UpdateMusic(float sliderValue)
	{
		settings = settings.WithMusic(sliderValue);
		settings.ApplyTo(Mixer);
		MusicPlayer.Instance.SetVolume(sliderValue);
	}

	public void UpdateAmbience(float sliderValue)
	{
		settings = settings.WithAmbience(sliderValue);
		settings.ApplyTo(Mixer);
	}

	/// <summary>Persist the current values. Wired to the Save button.</summary>
	public void SaveSettings()
	{
		settings.Save();
	}
}
