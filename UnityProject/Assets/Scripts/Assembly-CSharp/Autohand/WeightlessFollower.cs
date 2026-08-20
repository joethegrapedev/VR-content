using System;
using System.Collections.Generic;
using UnityEngine;

namespace Autohand
{
	[DefaultExecutionOrder(1)]
	public class WeightlessFollower : MonoBehaviour
	{
		[HideInInspector]
		public Transform follow;

		[HideInInspector]
		public Transform follow1;

		public Dictionary<Hand, Transform> heldMoveTo = new Dictionary<Hand, Transform>();

		[HideInInspector]
		public float followPositionStrength = 30f;

		[HideInInspector]
		public float followRotationStrength = 30f;

		[HideInInspector]
		public float maxVelocity = 5f;

		[HideInInspector]
		public Grabbable grab;

		internal Rigidbody body;

		private Transform moveTo;

		private float startMass;

		private float startDrag;

		private float startAngleDrag;

		public void Start()
		{
			if (body == null)
			{
				body = GetComponent<Rigidbody>();
			}
			if (startAngleDrag == 0f)
			{
				startMass = body.mass;
				startDrag = body.drag;
				startAngleDrag = body.angularDrag;
			}
		}

		public virtual void Set(Hand hand, Grabbable grab)
		{
			if (!heldMoveTo.ContainsKey(hand))
			{
				heldMoveTo.Add(hand, new GameObject().transform);
				heldMoveTo[hand].name = "HELD FOLLOW POINT";
			}
			Transform transformRuler = AutoHandExtensions.transformRuler;
			transformRuler.position = hand.transform.position;
			transformRuler.rotation = hand.transform.rotation;
			Transform transformRulerChild = AutoHandExtensions.transformRulerChild;
			transformRulerChild.position = grab.transform.position;
			transformRulerChild.rotation = grab.transform.rotation;
			if (grab.maintainGrabOffset)
			{
				transformRuler.position = hand.follow.position + hand.grabPositionOffset;
				transformRuler.rotation = hand.follow.rotation * hand.grabRotationOffset;
			}
			else
			{
				transformRuler.position = hand.follow.position;
				transformRuler.rotation = hand.follow.rotation;
			}
			heldMoveTo[hand].parent = hand.moveTo;
			heldMoveTo[hand].position = transformRulerChild.position;
			heldMoveTo[hand].rotation = transformRulerChild.rotation;
			if (body == null)
			{
				body = GetComponent<Rigidbody>();
			}
			if (startAngleDrag == 0f)
			{
				startMass = body.mass;
				startDrag = body.drag;
				startAngleDrag = body.angularDrag;
			}
			body.drag = hand.body.drag;
			body.angularDrag = hand.body.angularDrag;
			if (follow == null)
			{
				follow = heldMoveTo[hand];
			}
			else if (follow1 == null)
			{
				follow1 = heldMoveTo[hand];
			}
			followPositionStrength = hand.followPositionStrength;
			followRotationStrength = hand.followRotationStrength;
			maxVelocity = hand.maxVelocity;
			this.grab = grab;
			if (moveTo == null)
			{
				moveTo = new GameObject().transform;
				moveTo.name = base.gameObject.name + " FOLLOW POINT";
				moveTo.parent = follow.parent;
			}
			hand.OnReleased += delegate(Hand hand2, Grabbable grabbable)
			{
				RemoveFollow(hand2, heldMoveTo[hand2]);
			};
		}

		public virtual void FixedUpdate()
		{
			if (!(follow == null))
			{
				MoveTo();
				TorqueTo();
			}
		}

		protected void SetMoveTo()
		{
			if (!(follow == null))
			{
				if ((bool)follow1)
				{
					moveTo.position = Vector3.Lerp(follow.position, follow1.position, 0.5f);
					moveTo.rotation = Quaternion.Lerp(follow.rotation, follow1.rotation, 0.5f);
				}
				else
				{
					moveTo.position = follow.position;
					moveTo.rotation = follow.rotation;
				}
			}
		}

		protected virtual void MoveTo()
		{
			if (!(followPositionStrength <= 0f) && !(body == null))
			{
				SetMoveTo();
				Vector3 position = moveTo.position;
				float num = Vector3.Distance(position, base.transform.position);
				if (grab.collisionTracker.collisionCount > 0)
				{
					float num2 = maxVelocity;
					Vector3 target = (position - base.transform.position).normalized * followPositionStrength * num;
					target.x = Mathf.Clamp(target.x, 0f - num2, num2);
					target.y = Mathf.Clamp(target.y, 0f - num2, num2);
					target.z = Mathf.Clamp(target.z, 0f - num2, num2);
					body.velocity = Vector3.MoveTowards(body.velocity, target, 0.5f + body.velocity.magnitude / num2);
				}
				else
				{
					float num3 = maxVelocity;
					Vector3 velocity = (position - base.transform.position).normalized * followPositionStrength * num;
					velocity.x = Mathf.Clamp(velocity.x, 0f - num3, num3);
					velocity.y = Mathf.Clamp(velocity.y, 0f - num3, num3);
					velocity.z = Mathf.Clamp(velocity.z, 0f - num3, num3);
					body.velocity = velocity;
				}
			}
		}

		protected virtual void TorqueTo()
		{
			if (body == null)
			{
				return;
			}
			(moveTo.rotation * Quaternion.Inverse(body.rotation)).ToAngleAxis(out var angle, out var axis);
			if (!float.IsInfinity(axis.x))
			{
				if (angle > 180f)
				{
					angle -= 360f;
				}
				Vector3 b = MathF.PI / 180f * angle * followRotationStrength * axis.normalized;
				if (CollisionCount() > 0)
				{
					body.angularVelocity = Vector3.Lerp(body.angularVelocity, b, 0.5f);
				}
				else
				{
					body.angularVelocity = Vector3.Lerp(body.angularVelocity, b, 0.95f);
				}
			}
		}

		private int CollisionCount()
		{
			return grab.CollisionCount();
		}

		public void RemoveFollow(Hand hand, Transform follow)
		{
			hand.OnReleased -= delegate(Hand hand2, Grabbable grab1)
			{
				RemoveFollow(hand2, heldMoveTo[hand2]);
			};
			if (this.follow == follow)
			{
				this.follow = null;
			}
			if (follow1 == follow)
			{
				follow1 = null;
			}
			if (this.follow == null && follow1 != null)
			{
				this.follow = follow1;
				follow1 = null;
			}
			if (this.follow == null && follow1 == null)
			{
				if (body != null)
				{
					body.mass = startMass;
					body.drag = startDrag;
					body.angularDrag = startAngleDrag;
				}
				UnityEngine.Object.Destroy(this);
			}
		}

		private void OnDestroy()
		{
			UnityEngine.Object.Destroy(moveTo.gameObject);
			foreach (KeyValuePair<Hand, Transform> item in heldMoveTo)
			{
				UnityEngine.Object.Destroy(item.Value.gameObject);
			}
			if (body != null)
			{
				body.mass = startMass;
				body.drag = startDrag;
				body.angularDrag = startAngleDrag;
			}
		}
	}
}
