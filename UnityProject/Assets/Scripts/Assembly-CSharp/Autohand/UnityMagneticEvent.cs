using System;
using UnityEngine.Events;

namespace Autohand
{
	[Serializable]
	public class UnityMagneticEvent : UnityEvent<MagneticSource, MagneticBody>
	{
	}
}
