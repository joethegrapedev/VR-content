using System;
using UnityEngine.Events;

namespace Autohand
{
	[Serializable]
	public class UnityHandGrabEvent : UnityEvent<Hand, Grabbable>
	{
	}
}
