using UnityEngine;

namespace Autohand.Demo
{
	public class JointBreakStopForce1 : MonoBehaviour
	{
		private void OnJointBreak(float breakForce)
		{
			if (base.gameObject.CanGetComponent<Rigidbody>(out var component))
			{
				component.velocity = Vector3.zero;
				component.angularVelocity = Vector3.zero;
			}
		}
	}
}
