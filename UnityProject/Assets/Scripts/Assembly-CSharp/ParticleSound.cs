using UnityEngine;

public class ParticleSound : MonoBehaviour
{
	public ParticleSystem ps;

	public AudioSource Sound;

	private void Start()
	{
		Sound = GetComponent<AudioSource>();
	}

	private void Update()
	{
	}

	public void playParticleNSound()
	{
		ps.Play();
		Sound.Play();
	}
}
