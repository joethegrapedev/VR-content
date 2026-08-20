using System;
using UnityEngine;

namespace Autohand.Demo
{
	public class TextGrabEvent : MonoBehaviour
	{
		public TextChanger changer;

		public Grabbable grab;

		[TextArea]
		public string message;

		private void Start()
		{
			if (grab == null && GetComponent<Grabbable>() != null)
			{
				grab = GetComponent<Grabbable>();
			}
			if (grab == null || changer == null)
			{
				UnityEngine.Object.Destroy(this);
			}
			Grabbable grabbable = grab;
			grabbable.OnGrabEvent = (HandGrabEvent)Delegate.Combine(grabbable.OnGrabEvent, new HandGrabEvent(OnGrab));
		}

		private void OnGrab(Hand hand, Grabbable grab)
		{
			changer?.UpdateText(message);
		}
	}
}
