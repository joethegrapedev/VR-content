using UnityEngine;

public class Indicator : MonoBehaviour
{
	public GameObject VIPTick;

	public GameObject DefaultTick;

	public void Activate3minIndicator()
	{
		VIPTick.SetActive(value: true);
		DefaultTick.SetActive(value: false);
	}

	public void Activate10minIndicator()
	{
		VIPTick.SetActive(value: false);
		DefaultTick.SetActive(value: true);
	}
}
