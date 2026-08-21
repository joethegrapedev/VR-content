using UnityEngine;

public class DoorScript : MonoBehaviour
{
	private Animator animator;

	private void Awake()
	{
		animator = GetComponent<Animator>();
	}

	public void OpenDoor()
	{
		GameAudioCues.DoorOpen(transform.position);
		animator.SetBool("Open", value: true);
	}

	public void CloseDoor()
	{
		GameAudioCues.DoorClose(transform.position);
		animator.SetBool("Open", value: false);
	}
}
