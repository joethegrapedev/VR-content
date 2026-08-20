using UnityEngine;

public class DetectPlayer : MonoBehaviour
{
	public MyRuntimeTest timer;

	private void Start()
	{
		timer.enabled = false;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Player")
		{
			timer.enabled = true;
			Debug.Log("Player detected, starting timer");
		}
		else
		{
			Debug.Log("not the player, timer not active");
		}
	}
}
