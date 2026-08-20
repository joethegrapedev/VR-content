using UnityEngine;
using UnityEngine.Events;

namespace Autohand.Demo
{
	public class XRControllerEvent : MonoBehaviour
	{
		public XRHandControllerLink link;

		public CommonButton button;

		public UnityEvent Pressed;

		public UnityEvent Released;

		private bool pressed;

		private void Update()
		{
			if (link.ButtonPressed(button) && !pressed)
			{
				Pressed?.Invoke();
				pressed = true;
			}
			else if (!link.ButtonPressed(button) && pressed)
			{
				Released?.Invoke();
				pressed = false;
			}
		}
	}
}
