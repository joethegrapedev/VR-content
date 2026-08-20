using System.Linq;
using UnityEngine;

namespace Autohand
{
	public class AutoHandPlayerForceArea : MonoBehaviour
	{
		public AutoHandPlayer player;

		public float force = 1f;

		public ForceMode forceMode;

		public LayerMask layers = -1;

		private Collider[] colliders = new Collider[30];

		private void FixedUpdate()
		{
			Vector3 vector = new Vector3 { [player.bodyCollider.direction] = 1f };
			float num = player.bodyCollider.height / 2f - player.bodyCollider.radius;
			Vector3 position = player.bodyCollider.center - vector * num;
			Vector3 position2 = player.bodyCollider.center + vector * num;
			Vector3 point = base.transform.TransformPoint(position);
			Vector3 point2 = base.transform.TransformPoint(position2);
			Vector3 r = base.transform.TransformVector(player.bodyCollider.radius, player.bodyCollider.radius, player.bodyCollider.radius);
			float radius = (from xyz in Enumerable.Range(0, 3)
				select (xyz != player.bodyCollider.direction) ? r[xyz] : 0f).Select(Mathf.Abs).Max();
			int num2 = Physics.OverlapCapsuleNonAlloc(point, point2, radius, colliders, layers);
			for (int num3 = 0; num3 < num2; num3++)
			{
				if (colliders[num3].attachedRigidbody != null && colliders[num3].attachedRigidbody != player.body && colliders[num3].attachedRigidbody != player.handRight.body && colliders[num3].attachedRigidbody != player.handLeft.body)
				{
					Vector3 vector2 = (colliders[num3].transform.position - player.transform.position).normalized * force;
					vector2.y /= 10f;
					colliders[num3].attachedRigidbody.AddForce(vector2, forceMode);
				}
			}
		}
	}
}
