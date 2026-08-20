using UnityEngine;

namespace Autohand.Demo
{
	public class ExplosionSource : MonoBehaviour
	{
		public float radius = 1f;

		public float force = 10f;

		public void Explode(bool destroy)
		{
			Collider[] array = Physics.OverlapSphere(base.transform.position, radius);
			for (int i = 0; i < array.Length; i++)
			{
				Rigidbody component = array[i].GetComponent<Rigidbody>();
				if (component != null)
				{
					component.AddExplosionForce(force, base.transform.position, radius);
				}
			}
			if (destroy)
			{
				Object.Destroy(base.gameObject);
			}
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(base.transform.position, radius);
		}
	}
}
