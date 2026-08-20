using UnityEngine;

public class Blood : MonoBehaviour
{
	public ParticleSystem blood;

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.tag == "Blood")
		{
			blood.Play();
		}
	}
}
