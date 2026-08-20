using UnityEngine;

namespace Autohand
{
	public struct SaveRigidbodyData
	{
		private GameObject origin;

		private float mass;

		private float angularDrag;

		private float drag;

		private bool useGravity;

		private bool isKinematic;

		private RigidbodyInterpolation interpolation;

		private CollisionDetectionMode collisionDetectionMode;

		private RigidbodyConstraints constraints;

		public SaveRigidbodyData(Rigidbody from, bool removeBody = true)
		{
			origin = from.gameObject;
			mass = from.mass;
			drag = from.drag;
			angularDrag = from.angularDrag;
			useGravity = from.useGravity;
			isKinematic = from.isKinematic;
			interpolation = from.interpolation;
			collisionDetectionMode = from.collisionDetectionMode;
			constraints = from.constraints;
			if (removeBody)
			{
				Object.Destroy(from);
			}
		}

		public Rigidbody ReloadRigidbody()
		{
			if (origin != null)
			{
				if (origin.CanGetComponent<Rigidbody>(out var component))
				{
					return component;
				}
				Rigidbody rigidbody = origin.AddComponent<Rigidbody>();
				rigidbody.mass = mass;
				rigidbody.drag = drag;
				rigidbody.angularDrag = angularDrag;
				rigidbody.useGravity = useGravity;
				rigidbody.isKinematic = isKinematic;
				rigidbody.interpolation = interpolation;
				rigidbody.collisionDetectionMode = collisionDetectionMode;
				rigidbody.constraints = constraints;
				origin = null;
				return rigidbody;
			}
			return null;
		}
	}
}
