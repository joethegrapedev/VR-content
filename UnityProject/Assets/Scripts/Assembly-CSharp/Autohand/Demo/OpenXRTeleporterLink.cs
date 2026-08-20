using UnityEngine;
using UnityEngine.InputSystem;

namespace Autohand.Demo
{
	public class OpenXRTeleporterLink : MonoBehaviour
	{
		public Teleporter hand;

		public InputActionProperty startTeleportAction;

		public InputActionProperty finishTeleportAction;

		private bool teleporting;

		private void OnEnable()
		{
			if (startTeleportAction.action != null)
			{
				startTeleportAction.action.Enable();
			}
			if (startTeleportAction.action != null)
			{
				startTeleportAction.action.performed += StartTeleportAction;
			}
			if (finishTeleportAction.action != null)
			{
				finishTeleportAction.action.Enable();
			}
			if (finishTeleportAction.action != null)
			{
				finishTeleportAction.action.performed += FinishTeleportAction;
			}
		}

		private void OnDisable()
		{
			if (startTeleportAction.action != null)
			{
				startTeleportAction.action.performed -= StartTeleportAction;
			}
			if (finishTeleportAction.action != null)
			{
				finishTeleportAction.action.performed -= FinishTeleportAction;
			}
		}

		private void StartTeleportAction(InputAction.CallbackContext a)
		{
			if (!teleporting)
			{
				hand.StartTeleport();
				teleporting = true;
			}
		}

		private void FinishTeleportAction(InputAction.CallbackContext a)
		{
			if (teleporting)
			{
				hand.Teleport();
				teleporting = false;
			}
		}
	}
}
