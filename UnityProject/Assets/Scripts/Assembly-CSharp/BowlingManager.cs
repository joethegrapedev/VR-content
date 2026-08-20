using System.Collections.Generic;
using UnityEngine;

public class BowlingManager : MonoBehaviour
{
	[Header("Bowling Ball Settings")]
	public GameObject bowlingBall;

	[Header("Bowling Pin Settings")]
	public Vector3 pinCenter;

	public float pinSpaceX;

	public float pinSpaceZ;

	public List<GameObject> pins = new List<GameObject>();

	public Vector3 ballPosition;

	private void Start()
	{
		ballPosition = bowlingBall.transform.position;
		ResetPins();
	}

	public void ResetBall()
	{
		bowlingBall.GetComponent<Rigidbody>().isKinematic = true;
		bowlingBall.transform.position = ballPosition;
		bowlingBall.GetComponent<Rigidbody>().isKinematic = false;
	}

	public void ResetPins()
	{
		int num = pins.Count / 2 - 2;
		int num2 = pins.Count / 2 - 2;
		int num3 = 0;
		float x = pinCenter.x;
		float num4 = pinCenter.z;
		for (int i = 0; i < pins.Count; i++)
		{
			pins[i].GetComponent<Rigidbody>().isKinematic = true;
			pins[i].transform.rotation = Quaternion.Euler(0f, 0f, 0f);
			pins[i].transform.position = new Vector3(x, pinCenter.y, num4);
			pins[i].GetComponent<Rigidbody>().isKinematic = false;
			num4 += pinSpaceZ;
			if (num3 == num2)
			{
				num2--;
				num3 = 0;
				num4 = pinCenter.z + pinSpaceZ / 2f * (float)(num - num2);
				x = pinCenter.x + pinSpaceX * (float)(num - num2);
			}
			else
			{
				num3++;
			}
		}
	}
}
