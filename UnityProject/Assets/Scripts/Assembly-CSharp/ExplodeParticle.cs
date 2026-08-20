using System.Collections;
using UnityEngine;

public class ExplodeParticle : MonoBehaviour
{
	public ParticleSystem ps;

	public void playParticle()
	{
		StartCoroutine(Delay());
	}

	private IEnumerator Delay()
	{
		ps.Play();
		yield return new WaitForSeconds(4f);
		ps.Stop();
	}
}
