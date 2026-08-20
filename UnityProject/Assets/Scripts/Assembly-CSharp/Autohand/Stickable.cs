using UnityEngine;
using UnityEngine.Events;

namespace Autohand
{
	public class Stickable : MonoBehaviour
	{
		[Header("Sticky Settings")]
		public Rigidbody body;

		[Tooltip("How strong the joint is between the stickable and this")]
		public float stickStrength = 1f;

		[Tooltip("Multiplyer for required stick speed to activate")]
		public float stickSpeedMultiplyer = 1f;

		[Tooltip("This index must match the sticky object to stick")]
		public int stickIndex;

		[Header("Event")]
		public UnityEvent OnStick;

		public UnityEvent EndStick;

		private Sticky stickSource;

		private void OnDrawGizmosSelected()
		{
			if (!body && (bool)GetComponent<Rigidbody>())
			{
				body = GetComponent<Rigidbody>();
			}
		}

		public void Stick(Sticky source)
		{
			stickSource = source;
			OnStick?.Invoke();
		}

		public void Unstick(Sticky source)
		{
			stickSource = null;
			EndStick?.Invoke();
		}

		public void ForceReleaseStick()
		{
			stickSource?.ForceRelease(this);
		}
	}
}
