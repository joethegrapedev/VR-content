using System;
using UnityEngine.Events;

namespace Autohand
{
	[Serializable]
	public class UnityPlacePointEvent : UnityEvent<PlacePoint, Grabbable>
	{
	}
}
