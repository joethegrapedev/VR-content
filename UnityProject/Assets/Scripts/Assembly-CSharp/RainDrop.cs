using System.Collections.Generic;
using UnityEngine;

public class RainDrop : MonoBehaviour
{
	public ParticleSystem m_particleSystem;

	public GameObject rippleEffect;

	private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();

	private void OnParticleCollision(GameObject other)
	{
		int num = m_particleSystem.GetCollisionEvents(other, collisionEvents);
		int num2 = 0;
		while (num2 < num)
		{
			Vector3 intersection = collisionEvents[num2].intersection;
			intersection.y -= 0.0125f;
			GameObject obj = Object.Instantiate(rippleEffect, intersection, Quaternion.identity);
			num2++;
			Object.Destroy(obj, 2f);
		}
	}
}
