using Michsky.MUIP;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Gives every button in the scene a hover and click sound.
///
/// Binding at runtime rather than editing each button asset means new or
/// dynamically spawned UI is covered automatically, and it avoids touching
/// the 80 recovered MUIP button entries individually.
/// </summary>
public class UiSoundBinder : MonoBehaviour
{
	public string hoverClip = "SFX_UI_Hover";

	public string clickClip = "SFX_UI_Click";

	[Range(0f, 1f)]
	public float volume = 0.8f;

	[Tooltip("Rescan for buttons created after the scene loaded. Off by " +
		"default: the initial bind already includes inactive objects, and a " +
		"repeating full-scene scan is not free on a standalone headset.")]
	public bool rebindOnInterval;

	public float rebindInterval = 10f;

	private float nextRebind;

	private void OnEnable()
	{
		Bind();
	}

	private void Update()
	{
		if (!rebindOnInterval || Time.unscaledTime < nextRebind)
		{
			return;
		}
		nextRebind = Time.unscaledTime + Mathf.Max(rebindInterval, 0.5f);
		Bind();
	}

	/// <summary>Attach sounds to any button not already carrying them.</summary>
	public void Bind()
	{
		BindMuipButtons();
		BindStandardButtons();
	}

	private void BindMuipButtons()
	{
		AudioClip hover = AudioLibrary.Load(AudioLibrary.SfxFolder, hoverClip);
		AudioClip click = AudioLibrary.Load(AudioLibrary.SfxFolder, clickClip);
		if (hover == null && click == null)
		{
			return;
		}

		ButtonManager[] buttons = Object.FindObjectsOfType<ButtonManager>(true);
		for (int i = 0; i < buttons.Length; i++)
		{
			ButtonManager button = buttons[i];
			if (button == null)
			{
				continue;
			}
			if (button.soundSource == null)
			{
				button.soundSource = EnsureSource(button.gameObject);
			}
			// The recovered project ships these disabled with null clips.
			button.hoverSound = hover;
			button.clickSound = click;
			button.useHoverSound = hover != null;
			button.useClickSound = click != null;
			button.enableButtonSounds = true;
		}
	}

	private void BindStandardButtons()
	{
		Button[] buttons = Object.FindObjectsOfType<Button>(true);
		for (int i = 0; i < buttons.Length; i++)
		{
			Button button = buttons[i];
			if (button == null || button.GetComponent<ButtonManager>() != null ||
				button.GetComponent<UiSoundMarker>() != null)
			{
				continue;
			}

			button.gameObject.AddComponent<UiSoundMarker>();
			button.onClick.AddListener(() => SfxPlayer.PlayUi(clickClip, volume));
			AddHoverTrigger(button.gameObject);
		}
	}

	private void AddHoverTrigger(GameObject host)
	{
		EventTrigger trigger = host.GetComponent<EventTrigger>();
		if (trigger == null)
		{
			trigger = host.AddComponent<EventTrigger>();
		}

		EventTrigger.Entry entry = new EventTrigger.Entry
		{
			eventID = EventTriggerType.PointerEnter
		};
		entry.callback.AddListener((BaseEventData _) =>
			SfxPlayer.PlayUi(hoverClip, volume * 0.7f));
		trigger.triggers.Add(entry);
	}

	private AudioSource EnsureSource(GameObject host)
	{
		AudioSource source = host.GetComponent<AudioSource>();
		if (source == null)
		{
			source = host.AddComponent<AudioSource>();
		}
		source.playOnAwake = false;
		source.spatialBlend = 0f;
		source.outputAudioMixerGroup = AudioLibrary.Group(AudioChannels.Sfx);
		return source;
	}
}
