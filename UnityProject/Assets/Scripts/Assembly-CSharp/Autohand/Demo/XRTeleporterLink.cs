using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace Autohand.Demo
{
	public class XRTeleporterLink : MonoBehaviour
	{
		public Teleporter hand;

		public XRNode role;

		public CommonButton button;

		private bool teleporting;

		private InputDevice device;

		private List<InputDevice> devices;

		private void Start()
		{
			devices = new List<InputDevice>();
		}

		private void FixedUpdate()
		{
			InputDevices.GetDevicesAtXRNode(role, devices);
			if (devices.Count > 0)
			{
				device = devices[0];
			}
			_ = device;
			if (device.isValid && device.TryGetFeatureValue(XRHandControllerLink.GetCommonButton(button), out var value))
			{
				if (teleporting && !value)
				{
					hand.Teleport();
					teleporting = false;
				}
				else if (!teleporting && value)
				{
					hand.StartTeleport();
					teleporting = true;
				}
			}
		}
	}
}
