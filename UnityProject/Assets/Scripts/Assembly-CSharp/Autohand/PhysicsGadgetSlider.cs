using UnityEngine;
using UnityEngine.Events;

namespace Autohand
{
	public class PhysicsGadgetSlider : PhysicsGadgetConfigurableLimitReader
	{
		[Min(0.01f)]
		[Tooltip("The percentage (0-1) from the required value needed to call the event, if threshold is 0.1 OnMax will be called at 0.9, OnMin at -0.9, and OnMiddle at -0.1 or 0.1")]
		public float threshold = 0.05f;

		[Min(0f)]
		public int stepCount;

		public int startStep;

		private int prevStepCount = -1;

		public UnityEvent OnMax;

		public UnityEvent OnMid;

		public UnityEvent OnMin;

		public StepEvent[] stepEvents;

		private bool min;

		private bool max;

		private bool mid = true;

		private int currStep = -1;

		private int prevStep = -1;

		private float minimum;

		private float maximum;

		private float[] stepMarkers;

		protected void FixedUpdate()
		{
			float num = GetValue();
			if (!max && mid && num + threshold >= 1f)
			{
				Max();
			}
			if (!min && mid && num - threshold <= -1f)
			{
				Min();
			}
			if (num <= threshold && max && !mid)
			{
				Mid();
			}
			if (num >= 0f - threshold && min && !mid)
			{
				Mid();
			}
		}

		protected override void Start()
		{
			base.Start();
			if (startStep > 0)
			{
				FindSteps();
				SetSpring(startStep - 1);
			}
		}

		private void Update()
		{
			AdjustStep();
		}

		private void AdjustStep()
		{
			if (stepCount > 0)
			{
				FindSteps();
				SetSpring(FindCurrentStep());
			}
		}

		private bool FindSteps()
		{
			if (prevStepCount == stepCount)
			{
				return false;
			}
			prevStepCount = stepCount;
			stepMarkers = new float[stepCount];
			minimum = 0f - GetJoint().linearLimit.limit;
			maximum = GetJoint().linearLimit.limit;
			float step = GetStep();
			for (int i = 0; i < stepCount; i++)
			{
				stepMarkers[i] = minimum + (float)i * step;
			}
			return true;
		}

		public void SetSpring(int step)
		{
			currStep = step;
			Vector3 vector = new Vector3((joint.xMotion == ConfigurableJointMotion.Locked) ? 0f : stepMarkers[step], (joint.yMotion == ConfigurableJointMotion.Locked) ? 0f : stepMarkers[step], (joint.zMotion == ConfigurableJointMotion.Locked) ? 0f : stepMarkers[step]);
			GetJoint().transform.localPosition = vector;
			GetJoint().targetPosition = vector;
		}

		public void SetSpring(float stepRotation)
		{
			Vector3 targetPosition = new Vector3((joint.xMotion == ConfigurableJointMotion.Locked) ? 0f : stepRotation, (joint.yMotion == ConfigurableJointMotion.Locked) ? 0f : stepRotation, (joint.zMotion == ConfigurableJointMotion.Locked) ? 0f : stepRotation);
			GetJoint().targetPosition = targetPosition;
		}

		private float FindCurrentStep()
		{
			float num = GetValue() / GetScalar();
			for (int i = 0; i < stepCount; i++)
			{
				if (num >= GetMinimumStep(i) && num <= GetMaximumStep(i))
				{
					currStep = i;
					if (currStep != prevStep)
					{
						Step();
						prevStep = currStep;
					}
					return stepMarkers[i];
				}
			}
			return 0f;
		}

		private float GetStep()
		{
			return (Mathf.Abs(minimum) + Mathf.Abs(maximum)) / (float)(stepCount - 1);
		}

		private float GetScalar()
		{
			return 1f / Mathf.Abs(minimum);
		}

		private float GetMinimumStep(int index)
		{
			return stepMarkers[index] - GetStep() / 2f;
		}

		private float GetMaximumStep(int index)
		{
			return stepMarkers[index] + GetStep() / 2f;
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

		private void Step()
		{
			for (int i = 0; i < stepEvents.Length; i++)
			{
				if (stepEvents[i].step == currStep + 1)
				{
					stepEvents[i].OnStepEnter?.Invoke();
				}
				else if (stepEvents[i].step == prevStep + 1)
				{
					stepEvents[i].OnStepExit?.Invoke();
				}
			}
		}
	}
}
