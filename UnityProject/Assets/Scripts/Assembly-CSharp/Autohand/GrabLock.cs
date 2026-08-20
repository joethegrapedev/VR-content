using UnityEngine;
using UnityEngine.Events;

namespace Autohand
{
	[RequireComponent(typeof(Grabbable))]
	public class GrabLock : MonoBehaviour
	{
		[Header("Hand.Released() must be called elsewhere")]
		[Header("Use this script to prevent grabbable release")]
		public UnityEvent OnGrabPressed;
	}
}
