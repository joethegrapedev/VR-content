using System;
using UnityEngine;

namespace Autohand
{
	[RequireComponent(typeof(Grabbable))]
	public class GrabbableExtraEvents : MonoBehaviour
	{
		public UnityHandGrabEvent OnFirstGrab;

		public UnityHandGrabEvent OnLastRelease;

		public UnityHandGrabEvent OnTwoHandedGrab;

		public UnityHandGrabEvent OnTwoHandedRelease;

		private Grabbable grab;

		private void OnEnable()
		{
			grab = GetComponent<Grabbable>();
			Grabbable grabbable = grab;
			grabbable.OnGrabEvent = (HandGrabEvent)Delegate.Combine(grabbable.OnGrabEvent, new HandGrabEvent(Grab));
			Grabbable grabbable2 = grab;
			grabbable2.OnReleaseEvent = (HandGrabEvent)Delegate.Combine(grabbable2.OnReleaseEvent, new HandGrabEvent(Release));
		}

		private void OnDisable()
		{
			grab = grab ?? GetComponent<Grabbable>();
			Grabbable grabbable = grab;
			grabbable.OnGrabEvent = (HandGrabEvent)Delegate.Remove(grabbable.OnGrabEvent, new HandGrabEvent(Grab));
			Grabbable grabbable2 = grab;
			grabbable2.OnReleaseEvent = (HandGrabEvent)Delegate.Remove(grabbable2.OnReleaseEvent, new HandGrabEvent(Release));
		}

		public void Grab(Hand hand, Grabbable grab)
		{
			if (grab.HeldCount() == 1)
			{
				OnFirstGrab?.Invoke(hand, grab);
			}
			if (grab.HeldCount() == 2)
			{
				OnTwoHandedGrab?.Invoke(hand, grab);
			}
		}

		public void Release(Hand hand, Grabbable grab)
		{
			if (grab.HeldCount() == 0)
			{
				OnLastRelease?.Invoke(hand, grab);
			}
			if (grab.HeldCount() == 1)
			{
				OnTwoHandedRelease?.Invoke(hand, grab);
			}
		}
	}
}
