using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Autohand
{
	[RequireComponent(typeof(Rigidbody), typeof(Grabbable))]
	public class GrabbableThrowEvent : MonoBehaviour
	{
		[Tooltip("The velocity magnitude required on collision to cause the break event")]
		public float breakVelocity = 1f;

		[Tooltip("The layers that will cause this grabbale to break")]
		public LayerMask collisionLayers = -1;

		public UnityEvent OnBreak;

		private Rigidbody rb;

		private Grabbable grab;

		private bool thrown;

		private Coroutine resetThrowing;

		private float throwTime = 3f;

		private void Awake()
		{
			rb = GetComponent<Rigidbody>();
			grab = GetComponent<Grabbable>();
		}

		private void OnEnable()
		{
			Grabbable grabbable = grab;
			grabbable.OnReleaseEvent = (HandGrabEvent)Delegate.Combine(grabbable.OnReleaseEvent, new HandGrabEvent(OnReleased));
		}

		private void OnDisable()
		{
			Grabbable grabbable = grab;
			grabbable.OnReleaseEvent = (HandGrabEvent)Delegate.Remove(grabbable.OnReleaseEvent, new HandGrabEvent(OnReleased));
		}

		private void OnReleased(Hand hand, Grabbable grab)
		{
			if (rb.velocity.magnitude >= breakVelocity)
			{
				thrown = true;
			}
			if (resetThrowing != null)
			{
				StopCoroutine(resetThrowing);
			}
			resetThrowing = StartCoroutine(ResetThrown());
		}

		private IEnumerator ResetThrown()
		{
			yield return new WaitForSeconds(throwTime);
			thrown = false;
			resetThrowing = null;
		}

		private void OnCollisionEnter(Collision collision)
		{
			if (thrown && !(grab == null) && ((1 << collision.collider.gameObject.layer) & (int)collisionLayers) != 0 && rb.velocity.magnitude >= breakVelocity)
			{
				Invoke("Break", Time.fixedDeltaTime);
			}
		}

		private void Break()
		{
			OnBreak.Invoke();
		}
	}
}
