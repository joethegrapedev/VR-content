using UnityEngine;

/// <summary>
/// Named sounds for gameplay moments.
///
/// Call sites say what happened ("the player chose correctly") rather than
/// which file to play, so the clip choice lives in one place and the
/// gameplay scripts stay readable.
/// </summary>
public static class GameAudioCues
{
	private const string TaskCorrectClip = "SFX_Task_Correct";

	private const string TaskWrongClip = "SFX_Task_Wrong";

	private const string GameStartClip = "SFX_Game_Start";

	private const string GameEndClip = "SFX_Game_End";

	private const string TeleportClip = "SFX_Teleport";

	private const string DoorOpenClip = "SFX_Door_Open";

	private const string DoorCloseClip = "SFX_Door_Close";

	private const string ExtinguisherClip = "SFX_Extinguisher_Spray";

	private const string HoldTickClip = "SFX_Countdown_Tick";

	private const string ConfirmClip = "SFX_UI_Confirm";

	private const string ErrorClip = "SFX_UI_Error";

	/// <summary>A correct safety choice.</summary>
	public static void TaskCorrect(Vector3 position)
	{
		SfxPlayer.PlayAt(TaskCorrectClip, position, 0.85f);
	}

	/// <summary>An unsafe choice.</summary>
	public static void TaskWrong(Vector3 position)
	{
		SfxPlayer.PlayAt(TaskWrongClip, position, 0.8f);
	}

	/// <summary>The run begins.</summary>
	public static void GameStart()
	{
		SfxPlayer.PlayUi(GameStartClip, 0.85f);
	}

	/// <summary>The run is over, however it ended.</summary>
	public static void GameEnd()
	{
		SfxPlayer.PlayUi(GameEndClip, 0.9f);
	}

	public static void Teleport(Vector3 position)
	{
		SfxPlayer.PlayAt(TeleportClip, position, 0.7f);
	}

	public static void DoorOpen(Vector3 position)
	{
		SfxPlayer.PlayAt(DoorOpenClip, position, 0.8f);
	}

	public static void DoorClose(Vector3 position)
	{
		SfxPlayer.PlayAt(DoorCloseClip, position, 0.8f);
	}

	/// <summary>Extinguisher discharge onto a fire.</summary>
	public static void Extinguish(Vector3 position)
	{
		SfxPlayer.PlayAt(ExtinguisherClip, position, 0.7f);
	}

	/// <summary>One tick of a hold-to-activate interaction.</summary>
	public static void HoldTick(Vector3 position)
	{
		SfxPlayer.PlayAt(HoldTickClip, position, 0.4f);
	}

	/// <summary>A hold-to-activate interaction completed.</summary>
	public static void Confirm()
	{
		SfxPlayer.PlayUi(ConfirmClip, 0.8f);
	}

	/// <summary>An invalid or unsafe interaction.</summary>
	public static void Error()
	{
		SfxPlayer.PlayUi(ErrorClip, 0.75f);
	}
}
