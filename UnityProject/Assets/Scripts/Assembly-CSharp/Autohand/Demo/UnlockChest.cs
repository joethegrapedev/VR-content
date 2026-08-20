using System;
using UnityEngine;

namespace Autohand.Demo
{
	public class UnlockChest : MonoBehaviour
	{
		public PlacePoint placePoint;

		public HingeJoint joint;

		public void Start()
		{
			PlacePoint obj = placePoint;
			obj.OnPlaceEvent = (PlacePointEvent)Delegate.Combine(obj.OnPlaceEvent, (PlacePointEvent)delegate(PlacePoint point, Grabbable grab)
			{
				Unlock();
				grab.body.detectCollisions = false;
			});
		}

		public void Unlock()
		{
			joint.limits = new JointLimits
			{
				bounceMinVelocity = joint.limits.bounceMinVelocity,
				bounciness = joint.limits.bounciness,
				contactDistance = joint.limits.contactDistance,
				min = 0f,
				max = 160f
			};
			joint.spring = new JointSpring
			{
				spring = 5f,
				targetPosition = 160f
			};
		}

		public void Lock()
		{
			joint.limits = new JointLimits
			{
				bounceMinVelocity = joint.limits.bounceMinVelocity,
				bounciness = joint.limits.bounciness,
				contactDistance = joint.limits.contactDistance,
				min = -2f,
				max = 2f
			};
		}
	}
}
