using UnityEngine;

namespace Autohand.Demo
{
	public class XRAutoHandAxisFingerBender : MonoBehaviour
	{
		public XRHandControllerLink controller;

		public CommonAxis axis;

		[HideInInspector]
		public float[] bendOffsets;

		private float lastAxis;

		private void LateUpdate()
		{
			float num = controller.GetAxis(axis);
			for (int i = 0; i < controller.hand.fingers.Length; i++)
			{
				controller.hand.fingers[i].bendOffset += (num - lastAxis) * bendOffsets[i];
			}
			lastAxis = num;
		}

		private void OnDrawGizmosSelected()
		{
			if (controller == null && (bool)GetComponent<XRHandControllerLink>())
			{
				controller = GetComponent<XRHandControllerLink>();
				bendOffsets = new float[controller.hand.fingers.Length];
			}
		}
	}
}
