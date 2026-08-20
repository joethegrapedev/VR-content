using System;
using UnityEngine;
using UnityEngine.Events;

namespace Autohand.Demo
{
	public class Grenade : MonoBehaviour
	{
		public Grabbable grenade;

		public Grabbable pin;

		public ConfigurableJoint pinJoint;

		public float explosionDelay = 2f;

		public bool startDelayOnRelease;

		public float explosionForce = 100f;

		public float explosionRadius = 10f;

		public float pinJointStrength = 750f;

		public GameObject explosionEffect;

		public UnityEvent pinBreakEvent;

		public UnityEvent explosionEvent;

		private void OnEnable()
		{
			pin.isGrabbable = false;
			Grabbable grabbable = grenade;
			grabbable.OnGrabEvent = (HandGrabEvent)Delegate.Combine(grabbable.OnGrabEvent, new HandGrabEvent(OnGrenadeGrab));
			Grabbable grabbable2 = grenade;
			grabbable2.OnReleaseEvent = (HandGrabEvent)Delegate.Combine(grabbable2.OnReleaseEvent, new HandGrabEvent(OnGrenadeRelease));
			Grabbable grabbable3 = pin;
			grabbable3.OnGrabEvent = (HandGrabEvent)Delegate.Combine(grabbable3.OnGrabEvent, new HandGrabEvent(OnPinGrab));
			Grabbable grabbable4 = pin;
			grabbable4.OnReleaseEvent = (HandGrabEvent)Delegate.Combine(grabbable4.OnReleaseEvent, new HandGrabEvent(OnPinRelease));
			if (!grenade.jointedBodies.Contains(pin.body))
			{
				grenade.jointedBodies.Add(pin.body);
			}
			if (!pin.jointedBodies.Contains(grenade.body))
			{
				pin.jointedBodies.Add(grenade.body);
			}
		}

		private void OnDisable()
		{
			Grabbable grabbable = grenade;
			grabbable.OnGrabEvent = (HandGrabEvent)Delegate.Remove(grabbable.OnGrabEvent, new HandGrabEvent(OnGrenadeGrab));
			Grabbable grabbable2 = grenade;
			grabbable2.OnReleaseEvent = (HandGrabEvent)Delegate.Remove(grabbable2.OnReleaseEvent, new HandGrabEvent(OnGrenadeRelease));
			Grabbable grabbable3 = pin;
			grabbable3.OnGrabEvent = (HandGrabEvent)Delegate.Remove(grabbable3.OnGrabEvent, new HandGrabEvent(OnPinGrab));
			Grabbable grabbable4 = pin;
			grabbable4.OnReleaseEvent = (HandGrabEvent)Delegate.Remove(grabbable4.OnReleaseEvent, new HandGrabEvent(OnPinRelease));
		}

		private void OnGrenadeGrab(Hand hand, Grabbable grab)
		{
			if (pinJoint != null)
			{
				pin.isGrabbable = true;
			}
		}

		private void OnGrenadeRelease(Hand hand, Grabbable grab)
		{
			if (pinJoint != null)
			{
				pin.isGrabbable = false;
			}
			if (grenade != null && startDelayOnRelease)
			{
				Invoke("CheckJointBreak", explosionDelay + Time.fixedDeltaTime * 3f);
			}
		}

		private void OnPinGrab(Hand hand, Grabbable grab)
		{
			if (pinJoint != null)
			{
				pinJoint.breakForce = pinJointStrength;
			}
		}

		private void OnPinRelease(Hand hand, Grabbable grab)
		{
			if (pinJoint != null)
			{
				pinJoint.breakForce = 100000f;
			}
		}

		private void OnJointBreak(float breakForce)
		{
			Invoke("CheckJointBreak", Time.fixedDeltaTime * 2f);
		}

		private void CheckJointBreak()
		{
			if (pinJoint == null)
			{
				pin.maintainGrabOffset = false;
				pin.RemoveJointedBody(grenade.body);
				grenade.RemoveJointedBody(pin.body);
				if (!startDelayOnRelease)
				{
					Invoke("Explode", explosionDelay);
				}
			}
		}

		private void Explode()
		{
			Collider[] array = Physics.OverlapSphere(grenade.transform.position, explosionRadius);
			foreach (Collider collider in array)
			{
				if (AutoHandPlayer.Instance.body == collider.attachedRigidbody)
				{
					AutoHandPlayer.Instance.DisableGrounding(0.05f);
					float num = Vector3.Distance(collider.attachedRigidbody.position, grenade.transform.position);
					explosionForce *= 2f;
					collider.attachedRigidbody.AddExplosionForce(explosionForce - explosionForce * (num / explosionRadius), grenade.transform.position, explosionRadius);
					explosionForce /= 2f;
				}
				if (collider.attachedRigidbody != null)
				{
					float num2 = Vector3.Distance(collider.attachedRigidbody.position, grenade.transform.position);
					collider.attachedRigidbody.AddExplosionForce(explosionForce - explosionForce * (num2 / explosionRadius), grenade.transform.position, explosionRadius);
				}
			}
			explosionEvent?.Invoke();
			UnityEngine.Object.Instantiate(explosionEffect, grenade.transform.position, grenade.transform.rotation);
			UnityEngine.Object.Destroy(grenade.gameObject);
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.red;
			if (grenade != null)
			{
				Gizmos.DrawWireSphere(grenade.transform.position, explosionRadius);
			}
		}
	}
}
