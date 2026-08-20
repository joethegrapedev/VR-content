using System.Collections;
using UnityEngine;

namespace Autohand
{
	public class HandClippingGuard : MonoBehaviour
	{
		public Hand hand;

		[Tooltip("This should be a sphere collider that covers the hand (similar, but seperate from the recommended trigger sphere collider)")]
		public SphereCollider collisionGuard;

		public Transform body;

		public float guardTime = 0.02f;

		private Vector3 grabPoint;

		private bool runProtection;

		private Coroutine guardRoutine;

		private void Start()
		{
			collisionGuard.enabled = false;
			hand.OnGrabJointBreak += OnRelease;
			hand.OnBeforeGrabbed += BeforeGrab;
		}

		private void BeforeGrab(Hand hand, Grabbable grab)
		{
			if (body == null)
			{
				body = hand.transform.parent;
			}
			if (grab.ignoreReleaseTime == 0f)
			{
				runProtection = true;
			}
			else
			{
				runProtection = false;
			}
			grabPoint = hand.transform.position;
			if (guardRoutine != null)
			{
				StopCoroutine(guardRoutine);
				collisionGuard.enabled = false;
			}
		}

		private void OnRelease(Hand hand, Grabbable grab)
		{
			if (runProtection)
			{
				guardRoutine = StartCoroutine(Guard(hand));
				runProtection = false;
			}
		}

		private IEnumerator Guard(Hand hand)
		{
			hand.body.position = grabPoint;
			hand.transform.position = grabPoint;
			hand.transform.position = Vector3.MoveTowards(hand.transform.position, body.position, collisionGuard.radius);
			collisionGuard.enabled = true;
			yield return new WaitForSeconds(guardTime);
			collisionGuard.enabled = false;
		}
	}
}
