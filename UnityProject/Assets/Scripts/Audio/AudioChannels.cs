using UnityEngine;

/// <summary>
/// Names of the exposed AudioMixer parameters and the conversion between the
/// linear 0-1 values the UI sliders use and the decibels the mixer expects.
/// </summary>
public static class AudioChannels
{
	public const string Master = "Master";

	public const string Dialogue = "Dialogue";

	public const string Sfx = "SFX";

	public const string Music = "Music";

	public const string Ambience = "Ambience";

	/// <summary>Floor for the linear volume; log10(0) is negative infinity.</summary>
	public const float MinLinear = 0.0001f;

	public const float DefaultDialogue = 0.8f;

	public const float DefaultSfx = 0.8f;

	public const float DefaultMusic = 0.45f;

	public const float DefaultAmbience = 0.6f;

	/// <summary>Convert a 0-1 slider value to mixer decibels.</summary>
	public static float ToDecibels(float linear)
	{
		return Mathf.Log10(Mathf.Clamp(linear, MinLinear, 1f)) * 20f;
	}

	/// <summary>Convert mixer decibels back to a 0-1 slider value.</summary>
	public static float ToLinear(float decibels)
	{
		return Mathf.Clamp01(Mathf.Pow(10f, decibels / 20f));
	}
}
