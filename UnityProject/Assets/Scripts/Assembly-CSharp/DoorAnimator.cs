using UnityEngine;

public class DoorAnimator : MonoBehaviour
{
	[SerializeField]
	private DoorScript door;

	private void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Player")
		{
			door.OpenDoor();
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.tag == "Player")
		{
			door.CloseDoor();
		}
	}
}
