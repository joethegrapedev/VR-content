using UnityEngine;

namespace Autohand
{
	[RequireComponent(typeof(HingeJoint))]
	public class PhysicsGadgetHingeAngleReader : MonoBehaviour
	{
		public bool invertValue;

		[Tooltip("For objects slightly off center. \nThe minimum abs value required to return a value nonzero value\n - if playRange is 0.1, you have to move the gadget 10% to get a result")]
		public float playRange = 0.05f;

		private HingeJoint joint;

		protected float value;

		private Quaternion startRot;

		private Quaternion deltaParentRotation;

		protected virtual void Start()
		{
			joint = GetComponent<HingeJoint>();
			startRot = base.transform.localRotation;
		}

		public float GetValue()
		{
			value = joint.angle / (joint.limits.max - joint.limits.min) * 2f;
			value = (invertValue ? (0f - value) : value);
			if (Mathf.Abs(value) < playRange)
			{
				value = 0f;
			}
			return Mathf.Clamp(value, -1f, 1f);
		}

		public HingeJoint GetJoint()
		{
			return joint;
		}
	}
}
