using UnityEngine;

namespace Autohand.Demo
{
	public class WheelRotator : PhysicsGadgetHingeAngleReader
	{
		public Transform move;

		public Vector3 angle;

		public bool useLocal;

		private void Update()
		{
			if (useLocal)
			{
				move.localRotation *= Quaternion.Euler(angle * Time.deltaTime * GetValue());
			}
			else
			{
				move.rotation *= Quaternion.Euler(angle * Time.deltaTime * GetValue());
			}
		}
	}
}
