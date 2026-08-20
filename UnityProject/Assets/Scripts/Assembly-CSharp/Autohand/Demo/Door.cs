using UnityEngine;
using UnityEngine.Events;

namespace Autohand.Demo
{
	public class Door : PhysicsGadgetHingeAngleReader
	{
		[Header("Door should start closed")]
		public Rigidbody body;

		private Vector3 closedPosition;

		private Quaternion closedRotation;

		[Tooltip("The door needs to reach this level of open before it can be reset")]
		public float minThreshold = 0.05f;

		public float midThreshold = 0.05f;

		[Tooltip("The door needs to reach this level of open before it can be reset")]
		public float maxThreshold = 0.05f;

		[Space]
		public UnityEvent OnMax;

		public UnityEvent OnMid;

		public UnityEvent OnMin;

		private bool min;

		private bool max;

		private bool mid = true;

		private void Awake()
		{
			if (!body && (bool)GetComponent<Rigidbody>())
			{
				body = GetComponent<Rigidbody>();
			}
			closedPosition = base.transform.position;
			closedRotation = base.transform.rotation;
		}

		protected void FixedUpdate()
		{
			if (!max && mid && GetValue() + maxThreshold >= 1f)
			{
				Max();
			}
			if (!min && mid && GetValue() - minThreshold <= -1f)
			{
				Min();
			}
			if (GetValue() <= midThreshold && max && !mid)
			{
				Mid();
			}
			if (GetValue() >= 0f - midThreshold && min && !mid)
			{
				Mid();
			}
		}

		private void Max()
		{
			mid = false;
			max = true;
			OnMax?.Invoke();
		}

		private void Mid()
		{
			min = false;
			max = false;
			mid = true;
			OnMid?.Invoke();
		}

		private void Min()
		{
			min = true;
			mid = false;
			OnMin?.Invoke();
		}

		public void ClosedDoor()
		{
			base.transform.position = closedPosition;
			base.transform.rotation = closedRotation;
			body.isKinematic = true;
		}

		private void OnDrawGizmosSelected()
		{
			if (!body && (bool)GetComponent<Rigidbody>())
			{
				body = GetComponent<Rigidbody>();
			}
		}
	}
}
