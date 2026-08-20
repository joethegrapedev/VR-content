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
		animator.SetBool("Open", value: true);
	}

	public void CloseDoor()
	{
		animator.SetBool("Open", value: false);
	}
}
