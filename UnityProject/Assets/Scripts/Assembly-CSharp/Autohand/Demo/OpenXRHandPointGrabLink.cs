using UnityEngine;
using UnityEngine.InputSystem;

namespace Autohand.Demo
{
	public class OpenXRHandPointGrabLink : MonoBehaviour
	{
		public HandDistanceGrabber pointGrab;

		public InputActionProperty pointAction;

		public InputActionProperty stopPointAction;

		public InputActionProperty selectAction;

		public InputActionProperty stopSelectAction;

		private void OnEnable()
		{
			if (pointAction.action != null)
			{
				pointAction.action.Enable();
			}
			if (pointAction.action != null)
			{
				pointAction.action.performed += OnPoint;
			}
			if (stopPointAction.action != null)
			{
				stopPointAction.action.Enable();
			}
			if (stopPointAction.action != null)
			{
				stopPointAction.action.performed += OnStopPoint;
			}
			if (selectAction.action != null)
			{
				selectAction.action.Enable();
			}
			if (selectAction.action != null)
			{
				selectAction.action.performed += OnSelect;
			}
			if (stopSelectAction.action != null)
			{
				stopSelectAction.action.Enable();
			}
			if (stopSelectAction.action != null)
			{
				stopSelectAction.action.performed += OnDeselect;
			}
		}

		private void OnDisable()
		{
			if (pointAction.action != null)
			{
				pointAction.action.performed -= OnPoint;
			}
			if (stopPointAction.action != null)
			{
				stopPointAction.action.performed -= OnStopPoint;
			}
			if (selectAction.action != null)
			{
				selectAction.action.performed -= OnSelect;
			}
			if (stopSelectAction.action != null)
			{
				stopSelectAction.action.performed -= OnDeselect;
			}
		}

		private void OnPoint(InputAction.CallbackContext e)
		{
			pointGrab.StartPointing();
		}

		private void OnStopPoint(InputAction.CallbackContext e)
		{
			pointGrab.StopPointing();
		}

		private void OnSelect(InputAction.CallbackContext e)
		{
			pointGrab.SelectTarget();
		}

		private void OnDeselect(InputAction.CallbackContext e)
		{
			pointGrab.CancelSelect();
		}
	}
}
