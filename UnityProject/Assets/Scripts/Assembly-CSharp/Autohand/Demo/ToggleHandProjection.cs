using UnityEngine;

namespace Autohand.Demo
{
	public class ToggleHandProjection : MonoBehaviour
	{
		public void DisableGripProjection()
		{
			HandProjector[] array = Object.FindObjectsOfType<HandProjector>(includeInactive: true);
			foreach (HandProjector handProjector in array)
			{
				handProjector.gameObject.SetActive(value: false);
				if (handProjector.useGrabTransition)
				{
					handProjector.enabled = false;
				}
			}
		}

		public void EnableGripProjection()
		{
			HandProjector[] array = Object.FindObjectsOfType<HandProjector>(includeInactive: true);
			foreach (HandProjector handProjector in array)
			{
				handProjector.gameObject.SetActive(value: true);
				if (handProjector.useGrabTransition)
				{
					handProjector.enabled = true;
				}
			}
		}

		public void DisableHighlightProjection()
		{
			HandProjector[] array = Object.FindObjectsOfType<HandProjector>(includeInactive: true);
			foreach (HandProjector handProjector in array)
			{
				handProjector.gameObject.SetActive(value: false);
				if (!handProjector.useGrabTransition)
				{
					handProjector.enabled = false;
				}
			}
		}

		public void EnableHighlightProjection()
		{
			HandProjector[] array = Object.FindObjectsOfType<HandProjector>(includeInactive: true);
			foreach (HandProjector handProjector in array)
			{
				handProjector.gameObject.SetActive(value: true);
				if (!handProjector.useGrabTransition)
				{
					handProjector.enabled = true;
				}
			}
		}
	}
}
