using UnityEngine;

public class TeleportTutorial : MonoBehaviour
{
	public ParticleSystem Confetti;

	public ParticleSystem Confetti2;

	public ParticleSystem Confetti3;

	public ParticleSystem Confetti4;

	public ParticleSystem Confetti5;

	private void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Player")
		{
			Confetti.Play();
			Confetti2.Play();
			Confetti3.Play();
			Confetti4.Play();
			Confetti5.Play();
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.tag == "Player")
		{
			Confetti.Stop();
			Confetti2.Stop();
			Confetti3.Stop();
			Confetti4.Stop();
			Confetti5.Stop();
		}
	}
}
