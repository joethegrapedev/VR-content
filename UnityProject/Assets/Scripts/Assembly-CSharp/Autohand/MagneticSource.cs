using System;
using System.Collections.Generic;
using UnityEngine;

namespace Autohand
{
	[Serializable]
	[RequireComponent(typeof(Rigidbody))]
	public class MagneticSource : MonoBehaviour
	{
		public MagnetEffect magneticEffect;

		public float strength = 10f;

		public float radius = 4f;

		public ForceMode forceMode;

		public AnimationCurve forceDistanceCurce = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		public int magneticIndex;

		public UnityMagneticEvent magneticEnter;

		public UnityMagneticEvent magneticExit;

		private Rigidbody body;

		private List<MagneticBody> magneticBodies = new List<MagneticBody>();

		private float radiusScale;

		private void Start()
		{
			body = GetComponent<Rigidbody>();
			radiusScale = ((base.transform.lossyScale.x < base.transform.lossyScale.y) ? base.transform.lossyScale.x : base.transform.lossyScale.y);
			radiusScale = ((radiusScale < base.transform.lossyScale.z) ? radiusScale : base.transform.lossyScale.z);
		}

		private void FixedUpdate()
		{
			foreach (MagneticBody magneticBody in magneticBodies)
			{
				float num = Vector3.Distance(base.transform.position, magneticBody.transform.position);
				if (num < radius * radiusScale)
				{
					float time = num / (radius * radiusScale + 0.0001f);
					float num2 = forceDistanceCurce.Evaluate(time) * magneticBody.strengthMultiplyer * strength;
					num2 *= (float)((magneticEffect != MagnetEffect.Repulsive) ? 1 : (-1));
					magneticBody.body.AddForce((base.transform.position - magneticBody.transform.position).normalized * num2, forceMode);
				}
			}
		}

		private void OnTriggerEnter(Collider other)
		{
			if (other.attachedRigidbody != null && other.CanGetComponent<MagneticBody>(out var component) && !magneticBodies.Contains(component) && component.magneticIndex == magneticIndex)
			{
				magneticBodies.Add(component);
				magneticEnter?.Invoke(this, component);
				component.magneticEnter?.Invoke(this, component);
			}
		}

		private void OnTriggerExit(Collider other)
		{
			if (other.attachedRigidbody != null && other.CanGetComponent<MagneticBody>(out var component) && magneticBodies.Contains(component))
			{
				magneticBodies.Remove(component);
				magneticExit?.Invoke(this, component);
				component.magneticExit?.Invoke(this, component);
			}
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.blue;
			float num = ((base.transform.lossyScale.x < base.transform.lossyScale.y) ? base.transform.lossyScale.x : base.transform.lossyScale.y);
			num = ((num < base.transform.lossyScale.z) ? num : base.transform.lossyScale.z);
			Gizmos.DrawWireSphere(base.transform.position, radius * num);
		}
	}
}
