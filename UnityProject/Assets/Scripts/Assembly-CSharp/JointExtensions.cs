using UnityEngine;

public static class JointExtensions
{
	public static Vector3 Angles(this ConfigurableJoint joint)
	{
		Quaternion quaternion = Quaternion.LookRotation(joint.secondaryAxis, Vector3.Cross(joint.axis, joint.secondaryAxis));
		Quaternion quaternion2 = Quaternion.Inverse(quaternion);
		Vector3 vector = ((!(joint.connectedBody != null)) ? (quaternion2 * joint.GetComponent<Rigidbody>().transform.rotation * quaternion).eulerAngles : (quaternion2 * Quaternion.Inverse(joint.connectedBody.rotation) * joint.GetComponent<Rigidbody>().transform.rotation * quaternion).eulerAngles);
		return new Vector3(to(vector.x), to(vector.z), to(vector.y));
		static float to(float v)
		{
			if (v > 180f)
			{
				v -= 360f;
			}
			return v;
		}
	}
}
