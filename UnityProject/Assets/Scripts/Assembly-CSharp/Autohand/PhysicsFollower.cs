using System;
using UnityEngine;

namespace Autohand
{
	[RequireComponent(typeof(Rigidbody))]
	[DefaultExecutionOrder(-1)]
	public class PhysicsFollower : MonoBehaviour
	{
		[Header("Follow Settings")]
		[Space]
		[Tooltip("Follow target, the hand will always try to match this transforms rotation and position with rigidbody movements")]
		public Transform follow;

		[Tooltip("Stops hand physics follow - to freeze from all forces change rigidbody to kinematic or change rigidbody constraints")]
		public bool freezePos;

		[Tooltip("Stops hand physics follow - to freeze from all forces change rigidbody to kinematic or change rigidbody constraints")]
		public bool freezeRot;

		[Tooltip("This will offset the position without offsetting the rotation pivot")]
		public Vector3 followPositionOffset;

		public Vector3 rotationOffset;

		[Tooltip("Follow target speed (This will cause jittering if turned too high)")]
		[Min(0f)]
		public float followPositionStrength = 30f;

		[Tooltip("Follow target rotation speed (This will cause jittering if turned too high)")]
		[Min(0f)]
		public float followRotationStrength = 30f;

		[Tooltip("The maximum allowed velocity of the hand")]
		[Min(0f)]
		public float maxVelocity = 5f;

		internal Rigidbody body;

		private Transform moveTo;

		public void Start()
		{
			Set();
		}

		public virtual void Set()
		{
			if (moveTo == null)
			{
				moveTo = new GameObject().transform;
				moveTo.name = base.gameObject.name + " FOLLOW POINT";
				moveTo.parent = follow.parent;
				moveTo.position = follow.transform.position;
				moveTo.rotation = follow.transform.rotation;
				body = GetComponent<Rigidbody>();
			}
		}

		public void Update()
		{
			OnUpdate();
		}

		protected virtual void OnUpdate()
		{
			if (!(follow == null))
			{
				moveTo.position = follow.position + base.transform.rotation * followPositionOffset;
				moveTo.rotation = follow.rotation * Quaternion.Euler(rotationOffset);
			}
		}

		public void FixedUpdate()
		{
			OnFixedUpdate();
		}

		protected virtual void OnFixedUpdate()
		{
			if (!(follow == null))
			{
				moveTo.position = follow.position + base.transform.rotation * followPositionOffset;
				moveTo.rotation = follow.rotation * Quaternion.Euler(rotationOffset);
				if (!freezePos)
				{
					MoveTo();
				}
				if (!freezeRot)
				{
					TorqueTo();
				}
			}
		}

		internal virtual void MoveTo()
		{
			if (!(followPositionStrength <= 0f))
			{
				Vector3 position = moveTo.position;
				float num = Vector3.Distance(position, base.transform.position);
				float num2 = maxVelocity;
				Vector3 velocity = (position - base.transform.position).normalized * followPositionStrength * num;
				velocity.x = Mathf.Clamp(velocity.x, 0f - num2, num2);
				velocity.y = Mathf.Clamp(velocity.y, 0f - num2, num2);
				velocity.z = Mathf.Clamp(velocity.z, 0f - num2, num2);
				body.velocity = velocity;
			}
		}

		internal virtual void TorqueTo()
		{
			Quaternion rotation = moveTo.rotation;
			float value = Quaternion.Angle(body.rotation, rotation);
			Quaternion quaternion = Quaternion.Lerp(body.rotation, rotation, Mathf.Clamp(value, 0f, 2f) / 4f);
			float num = 90f * followRotationStrength;
			float num2 = 60f;
			(quaternion * Quaternion.Inverse(base.transform.rotation)).ToAngleAxis(out var angle, out var axis);
			axis.Normalize();
			axis *= MathF.PI / 180f;
			Vector3 vector = num * axis * angle - num2 * body.angularVelocity;
			Quaternion quaternion2 = body.inertiaTensorRotation * base.transform.rotation;
			vector = Quaternion.Inverse(quaternion2) * vector;
			vector.Scale(body.inertiaTensor);
			vector = quaternion2 * vector;
			body.AddTorque(vector);
		}

		private void OnDestroy()
		{
			UnityEngine.Object.Destroy(moveTo.gameObject);
		}
	}
}
