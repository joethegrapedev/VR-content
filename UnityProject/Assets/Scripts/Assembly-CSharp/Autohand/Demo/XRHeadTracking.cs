using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace Autohand.Demo
{
	public class XRHeadTracking : MonoBehaviour
	{
		public TrackingOriginModeFlags mode = TrackingOriginModeFlags.TrackingReference;

		private void Start()
		{
			List<XRInputSubsystem> list = new List<XRInputSubsystem>();
			SubsystemManager.GetInstances(list);
			for (int i = 0; i < list.Count; i++)
			{
				list[i].TrySetTrackingOriginMode(mode);
			}
		}
	}
}
