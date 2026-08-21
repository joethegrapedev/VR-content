using System;

/// <summary>
/// The audio setup for one scene. Immutable: a scene's profile is looked up,
/// never edited in place.
/// </summary>
[Serializable]
public class SceneAudioProfile
{
	public readonly string SceneName;

	public readonly string MusicTrack;

	public readonly float MusicVolume;

	public readonly string AmbienceBed;

	public readonly float AmbienceVolume;

	public readonly bool Footsteps;

	public readonly bool PositionalEmitters;

	public SceneAudioProfile(string sceneName, string musicTrack, float musicVolume,
		string ambienceBed, float ambienceVolume, bool footsteps,
		bool positionalEmitters)
	{
		SceneName = sceneName;
		MusicTrack = musicTrack;
		MusicVolume = musicVolume;
		AmbienceBed = ambienceBed;
		AmbienceVolume = ambienceVolume;
		Footsteps = footsteps;
		PositionalEmitters = positionalEmitters;
	}
}

/// <summary>
/// Which music and ambience each scene gets.
///
/// Menu, Keyboard and Leaderboard deliberately share the menu theme so the
/// music runs unbroken while the player names themselves and reviews scores.
/// </summary>
public static class SceneAudioProfiles
{
	public const string Menu = "Menu";

	public const string Keyboard = "Keyboard";

	public const string Leaderboard = "Leaderboard";

	public const string Tutorial = "Tutorial";

	public const string Map = "5SD Map";

	public const string AudioSettingsScene = "Audio Settings";

	private static readonly SceneAudioProfile[] Profiles =
	{
		new SceneAudioProfile(Menu, "Music_Menu", 0.55f,
			"Amb_Menu_RoomTone", 0.5f, false, false),
		new SceneAudioProfile(Keyboard, "Music_Menu", 0.55f,
			"Amb_Menu_RoomTone", 0.5f, false, false),
		new SceneAudioProfile(Leaderboard, "Music_Leaderboard", 0.6f,
			"Amb_Menu_RoomTone", 0.4f, false, false),
		new SceneAudioProfile(AudioSettingsScene, "Music_Menu", 0.55f,
			"Amb_Menu_RoomTone", 0.5f, false, false),
		new SceneAudioProfile(Tutorial, "Music_Gameplay_Calm", 0.4f,
			"Amb_Warehouse_RoomTone", 0.7f, true, true),
		// The main map keeps music low so the safety narration stays legible.
		new SceneAudioProfile(Map, "Music_Gameplay_Calm", 0.32f,
			"Amb_Warehouse_RoomTone", 0.8f, true, true)
	};

	private static readonly SceneAudioProfile Fallback =
		new SceneAudioProfile(string.Empty, null, 0f, null, 0f, false, false);

	/// <summary>The profile for a scene, or a silent fallback if unknown.</summary>
	public static SceneAudioProfile For(string sceneName)
	{
		for (int i = 0; i < Profiles.Length; i++)
		{
			if (Profiles[i].SceneName == sceneName)
			{
				return Profiles[i];
			}
		}
		return Fallback;
	}
}
