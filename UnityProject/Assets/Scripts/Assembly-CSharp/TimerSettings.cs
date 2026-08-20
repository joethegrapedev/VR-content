using UnityEngine;

public class TimerSettings : MonoBehaviour
{
	public static float setTime = 600f;

	public void threeMinutes()
	{
		setTime = 180f;
	}

	public void tenMinutes()
	{
		setTime = 600f;
	}
}
