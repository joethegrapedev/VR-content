using UnityEngine;

public class ConSound : MonoBehaviour
{
	public AudioSource Sound;

	private void Start()
	{
		Sound = GetComponent<AudioSource>();
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.tag == "Blood")
		{
			Sound.Play();
		}
	}
}
