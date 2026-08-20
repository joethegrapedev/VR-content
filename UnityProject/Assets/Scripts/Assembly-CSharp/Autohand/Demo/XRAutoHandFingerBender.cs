using UnityEngine;

namespace Autohand.Demo
{
	public class XRAutoHandFingerBender : MonoBehaviour
	{
		public XRHandControllerLink controller;

		public CommonButton button;

		[HideInInspector]
		public float[] bendOffsets;

		private bool pressed;

		private void Update()
		{
			if (!pressed && controller.ButtonPressed(button))
			{
				pressed = true;
				for (int i = 0; i < controller.hand.fingers.Length; i++)
				{
					controller.hand.fingers[i].bendOffset += bendOffsets[i];
				}
			}
			else if (pressed && !controller.ButtonPressed(button))
			{
				pressed = false;
				for (int j = 0; j < controller.hand.fingers.Length; j++)
				{
					controller.hand.fingers[j].bendOffset -= bendOffsets[j];
				}
			}
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
