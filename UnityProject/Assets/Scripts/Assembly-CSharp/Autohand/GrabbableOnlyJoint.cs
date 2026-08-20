using System;
using UnityEngine;

namespace Autohand
{
	[RequireComponent(typeof(Grabbable))]
	public class GrabbableOnlyJoint : MonoBehaviour
	{
		public Grabbable jointedGrabbable;

		public bool resetOnRelease = true;

		private Grabbable localGrabbable;

		private Joint freezeJoint;

		private Vector3 localStartPosition;

		private Quaternion localStartRotation;

		private void Start()
		{
			localGrabbable = GetComponent<Grabbable>();
			Grabbable grabbable = localGrabbable;
			grabbable.OnGrabEvent = (HandGrabEvent)Delegate.Combine(grabbable.OnGrabEvent, new HandGrabEvent(OnGrab));
			Grabbable grabbable2 = localGrabbable;
			grabbable2.OnReleaseEvent = (HandGrabEvent)Delegate.Combine(grabbable2.OnReleaseEvent, new HandGrabEvent(OnRelease));
			localStartPosition = jointedGrabbable.transform.InverseTransformPoint(base.transform.position);
			localStartRotation = Quaternion.Inverse(jointedGrabbable.transform.rotation) * base.transform.rotation;
			freezeJoint = localGrabbable.gameObject.AddComponent<FixedJoint>().GetCopyOf(Resources.Load<FixedJoint>("DefaultJoint"));
			freezeJoint.anchor = Vector3.zero;
			freezeJoint.breakForce = float.PositiveInfinity;
			freezeJoint.breakTorque = float.PositiveInfinity;
			freezeJoint.connectedBody = jointedGrabbable.body;
		}

		private void OnGrab(Hand hand, Grabbable grab)
		{
			if (grab.GetHeldBy().Count == 1)
			{
				UnityEngine.Object.Destroy(freezeJoint);
				freezeJoint = null;
			}
		}

		private void OnRelease(Hand hand, Grabbable grab)
		{
			if (grab.GetHeldBy().Count == 0)
			{
				base.transform.position = jointedGrabbable.transform.TransformPoint(localStartPosition);
				base.transform.rotation = jointedGrabbable.transform.rotation * localStartRotation;
				localGrabbable.body.position = base.transform.position;
				localGrabbable.body.rotation = base.transform.rotation;
				Invoke("CreateJoint", Time.fixedDeltaTime + Time.deltaTime);
			}
		}

		private void LateUpdate()
		{
			if (freezeJoint != null)
			{
				base.transform.position = jointedGrabbable.transform.TransformPoint(localStartPosition);
				base.transform.rotation = jointedGrabbable.transform.rotation * localStartRotation;
			}
		}

		private void CreateJoint()
		{
			freezeJoint = localGrabbable.gameObject.AddComponent<FixedJoint>().GetCopyOf(Resources.Load<FixedJoint>("DefaultJoint"));
			freezeJoint.anchor = Vector3.zero;
			freezeJoint.breakForce = float.PositiveInfinity;
			freezeJoint.breakTorque = float.PositiveInfinity;
			freezeJoint.connectedBody = jointedGrabbable.body;
		}
	}
}
