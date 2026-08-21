using System;

/// <summary>
/// Attaches a looping ambience clip to scene objects whose name contains a
/// keyword — the faulty strip lights buzz, the vents hiss, and so on.
/// </summary>
[Serializable]
public class AmbienceEmitterRule
{
	public readonly string Keyword;

	public readonly string ClipName;

	public readonly float Volume;

	public readonly float MaxDistance;

	/// <summary>Cap per rule so a warehouse of 3,000 props cannot spawn 3,000 sources.</summary>
	public readonly int MaxCount;

	public AmbienceEmitterRule(string keyword, string clipName, float volume,
		float maxDistance, int maxCount)
	{
		Keyword = keyword;
		ClipName = clipName;
		Volume = volume;
		MaxDistance = maxDistance;
		MaxCount = maxCount;
	}
}

/// <summary>The emitter rules applied to the warehouse scenes.</summary>
public static class AmbienceEmitterRules
{
	public static readonly AmbienceEmitterRule[] Warehouse =
	{
		new AmbienceEmitterRule("vent", "Amb_Air_Vent", 0.35f, 12f, 6),
		new AmbienceEmitterRule("fan", "Amb_Air_Vent", 0.35f, 12f, 4),
		new AmbienceEmitterRule("conveyor", "Amb_Conveyor", 0.45f, 16f, 4),
		new AmbienceEmitterRule("forklift", "SFX_Forklift_Idle", 0.30f, 18f, 3),
		new AmbienceEmitterRule("machine", "Amb_Distant_Machinery", 0.35f, 18f, 4)
	};
}
