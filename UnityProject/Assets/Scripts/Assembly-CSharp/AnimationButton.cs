using UnityEngine;

public class AnimationButton : MonoBehaviour
{
	public Animator anim;

	private void Start()
	{
		anim = GetComponent<Animator>();
		GetComponent<Animator>().enabled = false;
	}

	private void Update()
	{
	}

	public void playAnimtion()
	{
		GetComponent<Animator>().enabled = true;
		anim.Play("");
	}
}
