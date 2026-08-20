using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Autohand
{
	public class IgnoreHandPlayerCollision : MonoBehaviour
	{
		public List<Collider> colliders;

		private IEnumerable Start()
		{
			yield return new WaitForFixedUpdate();
			yield return new WaitForEndOfFrame();
			yield return new WaitForFixedUpdate();
			yield return new WaitForEndOfFrame();
			ActivateIgnoreCollision();
		}

		public void ActivateIgnoreCollision()
		{
			foreach (Collider collider in colliders)
			{
				AutoHandPlayer.Instance.IgnoreCollider(collider, ignore: true);
			}
		}

		public void DeactivateIgnoreCollision()
		{
			foreach (Collider collider in colliders)
			{
				AutoHandPlayer.Instance.IgnoreCollider(collider, ignore: false);
			}
		}
	}
}
