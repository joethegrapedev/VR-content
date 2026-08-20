using System;
using UnityEngine;
using UnityEngine.Events;

namespace Autohand
{
	[Serializable]
	public class UnityCanvasPointerEvent : UnityEvent<Vector3, GameObject>
	{
	}
}
