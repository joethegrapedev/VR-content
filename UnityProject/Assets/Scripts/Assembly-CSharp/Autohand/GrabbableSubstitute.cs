using System;
using UnityEngine;

namespace Autohand
{
	[RequireComponent(typeof(Grabbable))]
	public class GrabbableSubstitute : MonoBehaviour
	{
		[Tooltip("Whether or not to disable this gameobject on grab")]
		public bool disableOnGrab = true;

		[Tooltip("If true, the substitute will return to the this local location and turn off and the local grabbable will turn back on")]
		public bool returnOnRelease;

		public Grabbable grabbableSubstitute;

		private Grabbable localGrabbable;

		private void Start()
		{
			localGrabbable = GetComponent<Grabbable>();
			Grabbable grabbable = localGrabbable;
			grabbable.OnGrabEvent = (HandGrabEvent)Delegate.Combine(grabbable.OnGrabEvent, new HandGrabEvent(OnGrabOriginal));
			Grabbable grabbable2 = grabbableSubstitute;
			grabbable2.OnReleaseEvent = (HandGrabEvent)Delegate.Combine(grabbable2.OnReleaseEvent, new HandGrabEvent(OnReleaseSub));
		}

		private void OnGrabOriginal(Hand hand, Grabbable grab)
		{
			hand.Release();
			grabbableSubstitute.gameObject.SetActive(value: true);
			hand.CreateGrabConnection(grabbableSubstitute, hand.transform.position, hand.transform.rotation, grab.transform.position, grab.transform.rotation, executeGrabEvents: true);
			if (disableOnGrab)
			{
				grab.gameObject.SetActive(value: false);
			}
		}

		private void OnReleaseSub(Hand hand, Grabbable grab)
		{
			if (returnOnRelease)
			{
				grabbableSubstitute.transform.position = localGrabbable.transform.position;
				grabbableSubstitute.transform.rotation = localGrabbable.transform.rotation;
				grabbableSubstitute.body.position = localGrabbable.body.position;
				grabbableSubstitute.body.rotation = localGrabbable.body.rotation;
				grabbableSubstitute.gameObject.SetActive(value: false);
				if (disableOnGrab)
				{
					grab.gameObject.SetActive(value: true);
				}
			}
		}

		public void LocalSubstitute(Hand hand, Grabbable grab)
		{
			if (localGrabbable.gameObject.activeInHierarchy)
			{
				grabbableSubstitute.gameObject.SetActive(value: true);
				grabbableSubstitute.transform.position = localGrabbable.transform.position;
				grabbableSubstitute.transform.rotation = localGrabbable.transform.rotation;
				grabbableSubstitute.body.position = localGrabbable.body.position;
				grabbableSubstitute.body.rotation = localGrabbable.body.rotation;
				if (disableOnGrab)
				{
					localGrabbable.gameObject.SetActive(value: false);
				}
			}
		}
	}
}
