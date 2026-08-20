using UnityEngine;

public class AutoHandSettings : ScriptableObject
{
	[Tooltip("Whether the popup should be ignored on launch or not")]
	public bool ignoreSetup;

	[Tooltip("-1 is custom, 0 is low, 1 is medium, 2 is high")]
	public float quality = -1f;

	public static void ClearSettings()
	{
		AutoHandSettings autoHandSettings = Resources.Load<AutoHandSettings>("AutoHandSettings");
		autoHandSettings.ignoreSetup = false;
		autoHandSettings.quality = -1f;
	}
}
