using UnityEngine;

namespace Autohand
{
	[RequireComponent(typeof(ConfigurableJoint))]
	public class PhysicsGadgetJoystick : MonoBehaviour
	{
		private ConfigurableJoint joint;

		public bool invertX;

		public bool invertY;

		[Tooltip("For objects slightly off center. \nThe minimum abs value required to return a value nonzero value\n - if playRange is 0.1, you have to move the gadget 10% to get a result")]
		public float playRange = 0.05f;

		private Vector2 xRange;

		private Vector2 zRange;

		private Vector2 value;

		private Vector3 jointRotation;

		private Rigidbody body;

		private void Start()
		{
			joint = GetComponent<ConfigurableJoint>();
			body = GetComponent<Rigidbody>();
		}

		public void FixedUpdate()
		{
			xRange = new Vector2(joint.lowAngularXLimit.limit, joint.highAngularXLimit.limit);
			zRange = new Vector2(0f - joint.angularZLimit.limit, joint.angularZLimit.limit);
			jointRotation = joint.Angles();
			value = new Vector2(jointRotation.z / (zRange.x - zRange.y), jointRotation.x / (xRange.x - xRange.y)) * 2f;
		}

		public Vector2 GetValue()
		{
			if (Mathf.Abs(value.x) < playRange)
			{
				value.x = 0f;
			}
			if (Mathf.Abs(value.y) < playRange)
			{
				value.y = 0f;
			}
			value.x = (invertX ? (0f - value.x) : value.x);
			value.y = (invertY ? (0f - value.y) : value.y);
			return new Vector2(Mathf.Clamp(value.x, -1f, 1f), Mathf.Clamp(value.y, -1f, 1f));
		}
	}
}
