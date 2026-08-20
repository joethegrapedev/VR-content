using UnityEngine;
using UnityEngine.Events;

namespace Autohand
{
	public class PhysicsGadgetButton : PhysicsGadgetConfigurableLimitReader
	{
		private bool pressed;

		[Tooltip("The percentage (0-1) from the required value needed to call the event, if threshold is 0.1 OnPressed will be called at 0.9, and OnUnpressed at 0.1")]
		[Min(0.01f)]
		public float threshold = 0.1f;

		public bool lockOnPressed;

		[Space]
		public UnityEvent OnPressed;

		public UnityEvent OnUnpressed;

		private Vector3 startPos;

		private Vector3 pressedPos;

		private float pressedValue;

		protected new void Start()
		{
			base.Start();
			startPos = base.transform.localPosition;
		}

		protected void FixedUpdate()
		{
			float num = GetValue();
			if (!pressed && num + threshold >= 1f)
			{
				Pressed();
			}
			else if (!lockOnPressed && pressed && num - threshold <= 0f)
			{
				Unpressed();
			}
			if (num < 0f)
			{
				base.transform.localPosition = startPos;
			}
			if (pressed && lockOnPressed && num + threshold < pressedValue)
			{
				base.transform.localPosition = pressedPos;
			}
		}

		public void Pressed()
		{
			pressed = true;
			pressedValue = GetValue();
			pressedPos = base.transform.localPosition;
			OnPressed?.Invoke();
		}

		public void Unpressed()
		{
			pressed = false;
			OnUnpressed?.Invoke();
		}

		public void Unlock()
		{
			lockOnPressed = false;
			GetComponent<Rigidbody>().WakeUp();
		}
	}
}
