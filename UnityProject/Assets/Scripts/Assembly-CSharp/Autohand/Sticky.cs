using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Autohand
{
	[RequireComponent(typeof(Rigidbody))]
	public class Sticky : MonoBehaviour
	{
		[Header("Sticky Settings")]
		[Tooltip("How strong the joint is between the stickable and this")]
		public float stickStrength = 1f;

		[Tooltip("Multiplyer for required stick speed to activate")]
		public float requiredStickSpeed = 1f;

		[Tooltip("This index must match the stickable object to stick")]
		public int stickIndex;

		[Header("Event")]
		public UnityEvent OnStick;

		private Rigidbody body;

		private List<Stickable> stickers;

		private List<Joint> joints;

		private void Start()
		{
			if (body == null)
			{
				body = GetComponent<Rigidbody>();
			}
			stickers = new List<Stickable>();
			joints = new List<Joint>();
		}

		private void OnCollisionEnter(Collision collision)
		{
			if (collision.gameObject.CanGetComponent<Stickable>(out var component))
			{
				CreateStick(component);
			}
		}

		private void CreateStick(Stickable sticker)
		{
			if (!stickers.Contains(sticker) && sticker.stickIndex == stickIndex && !(sticker.body.velocity.sqrMagnitude * sticker.stickSpeedMultiplyer < requiredStickSpeed))
			{
				FixedJoint fixedJoint = base.gameObject.AddComponent<FixedJoint>();
				fixedJoint.connectedBody = sticker.body;
				fixedJoint.breakForce = 1000f * stickStrength * sticker.stickStrength;
				fixedJoint.breakTorque = 1000f * stickStrength * sticker.stickStrength;
				fixedJoint.connectedMassScale = 1f;
				fixedJoint.massScale = 1f;
				fixedJoint.enableCollision = false;
				fixedJoint.enablePreprocessing = true;
				sticker.Stick(this);
				OnStick?.Invoke();
				joints.Add(fixedJoint);
				stickers.Add(sticker);
			}
		}

		public void ForceRelease(Stickable stuck)
		{
			Object.Destroy(joints[stickers.IndexOf(stuck)]);
		}

		private void OnJointBreak(float breakForce)
		{
			StartCoroutine(JointBreak());
		}

		private IEnumerator JointBreak()
		{
			yield return new WaitForFixedUpdate();
			for (int num = joints.Count - 1; num >= 0; num--)
			{
				if (!joints[num])
				{
					joints.RemoveAt(num);
					stickers[num].EndStick?.Invoke();
					stickers.RemoveAt(num);
				}
			}
		}

		private void OnDrawGizmosSelected()
		{
			if (!body && (bool)GetComponent<Rigidbody>())
			{
				body = GetComponent<Rigidbody>();
			}
		}
	}
}
