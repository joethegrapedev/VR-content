using System;
using UnityEngine;
using UnityEngine.Events;

namespace Autohand.Demo
{
	[RequireComponent(typeof(Rigidbody))]
	public class Smasher : MonoBehaviour
	{
		private Rigidbody rb;

		[Header("Options")]
		public LayerMask smashableLayers;

		[Tooltip("How much to multiply the magnitude on smash")]
		public float forceMulti = 1f;

		[Tooltip("Can be left empty - The center of mass point to calculate velocity magnitude - for example: the camera of the hammer is a better point vs the pivot center of the hammer object")]
		public Transform centerOfMassPoint;

		[Header("Event")]
		public UnityEvent OnSmash;

		public SmashEvent OnSmashEvent;

		private Vector3[] velocityOverTime = new Vector3[3];

		private Vector3 lastPos;

		private void Start()
		{
			rb = GetComponent<Rigidbody>();
			if ((int)smashableLayers == 0)
			{
				smashableLayers = LayerMask.GetMask(Hand.grabbableLayerNameDefault);
			}
			OnSmashEvent = (SmashEvent)Delegate.Combine(OnSmashEvent, (SmashEvent)delegate
			{
				OnSmash?.Invoke();
			});
		}

		private void FixedUpdate()
		{
			for (int i = 1; i < velocityOverTime.Length; i++)
			{
				velocityOverTime[i] = velocityOverTime[i - 1];
			}
			velocityOverTime[0] = lastPos - (centerOfMassPoint ? centerOfMassPoint.position : rb.position);
			lastPos = (centerOfMassPoint ? centerOfMassPoint.position : rb.position);
		}

		private void OnCollisionEnter(Collision collision)
		{
			if (collision.transform.CanGetComponent<Smash>(out var component) && GetMagnitude() >= component.smashForce)
			{
				component.DoSmash();
				OnSmashEvent?.Invoke(this, component);
			}
		}

		private float GetMagnitude()
		{
			Vector3 zero = Vector3.zero;
			for (int i = 0; i < velocityOverTime.Length; i++)
			{
				zero += velocityOverTime[i];
			}
			return zero.magnitude / (float)velocityOverTime.Length * forceMulti * 10f;
		}
	}
}
