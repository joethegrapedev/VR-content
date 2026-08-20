using UnityEngine;

namespace Autohand
{
	[RequireComponent(typeof(ConfigurableJoint))]
	public class PhysicsGadgetConfigurableLimitReader : MonoBehaviour
	{
		public bool invertValue;

		[Tooltip("For objects slightly off center. \nThe minimum abs value required to return a value nonzero value\n - if playRange is 0.1, you have to move the gadget 10% to get a result")]
		public float playRange = 0.025f;

		protected ConfigurableJoint joint;

		protected Vector3 axisPos;

		private float value;

		private Vector3 limitAxis;

		protected virtual void Start()
		{
			joint = GetComponent<ConfigurableJoint>();
			limitAxis = new Vector3((joint.xMotion != ConfigurableJointMotion.Locked) ? 1 : 0, (joint.yMotion != ConfigurableJointMotion.Locked) ? 1 : 0, (joint.zMotion != ConfigurableJointMotion.Locked) ? 1 : 0);
			axisPos = Vector3.Scale(base.transform.localPosition, limitAxis);
		}

		public float GetValue()
		{
			bool flag = true;
			Vector3 b = Vector3.Scale(base.transform.localPosition, limitAxis);
			if (axisPos.x < b.x || axisPos.y < b.y || axisPos.z < b.z)
			{
				flag = false;
			}
			if (invertValue)
			{
				flag = !flag;
			}
			value = Vector3.Distance(axisPos, b) / joint.linearLimit.limit;
			if (!flag)
			{
				value *= -1f;
			}
			if (Mathf.Abs(value) < playRange)
			{
				value = 0f;
			}
			return Mathf.Clamp(value, -1f, 1f);
		}

		public ConfigurableJoint GetJoint()
		{
			return joint;
		}
	}
}
