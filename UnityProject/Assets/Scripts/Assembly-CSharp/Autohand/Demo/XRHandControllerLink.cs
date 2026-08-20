using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace Autohand.Demo
{
	public class XRHandControllerLink : HandControllerLink
	{
		public CommonButton grabButton = CommonButton.triggerButton;

		[Tooltip("This axis will bend all the fingers on the hand -> replaced with finger bender scripts")]
		public CommonAxis grabAxis;

		public CommonButton squeezeButton;

		private XRNode role;

		private bool squeezing;

		private bool grabbing;

		private InputDevice device;

		private List<InputDevice> devices = new List<InputDevice>();

		private void Start()
		{
			if (grabButton == squeezeButton)
			{
				Debug.LogError("AUTOHAND: You are using the same button for grab and squeeze on HAND CONTROLLER LINK, this may create conflict or errors", this);
			}
			if (hand.left)
			{
				role = XRNode.LeftHand;
			}
			else
			{
				role = XRNode.RightHand;
			}
			if (hand.left)
			{
				HandControllerLink.handLeft = this;
			}
			else
			{
				HandControllerLink.handRight = this;
			}
		}

		private void Update()
		{
			InputDevices.GetDevicesAtXRNode(role, devices);
			if (devices.Count > 0)
			{
				device = devices[0];
			}
			_ = device;
			if (!device.isValid)
			{
				return;
			}
			hand.SetGrip(GetAxis(grabAxis));
			if (device.TryGetFeatureValue(GetCommonButton(grabButton), out var value))
			{
				if (grabbing && !value)
				{
					hand.Release();
					grabbing = false;
				}
				else if (!grabbing && value)
				{
					hand.Grab();
					grabbing = true;
				}
			}
			if (device.TryGetFeatureValue(GetCommonButton(squeezeButton), out var value2))
			{
				if (squeezing && !value2)
				{
					hand.Unsqueeze();
					squeezing = false;
				}
				else if (!squeezing && value2)
				{
					hand.Squeeze();
					squeezing = true;
				}
			}
		}

		public List<InputDevice> Devices()
		{
			return devices;
		}

		public bool ButtonPressed(CommonButton button)
		{
			if (button == CommonButton.none)
			{
				return false;
			}
			if (device.TryGetFeatureValue(GetCommonButton(button), out var value))
			{
				return value;
			}
			return false;
		}

		public float GetAxis(CommonAxis axis)
		{
			if (axis == CommonAxis.none)
			{
				return 0f;
			}
			if (device.TryGetFeatureValue(GetCommonAxis(axis), out var value))
			{
				return value;
			}
			return 0f;
		}

		public Vector2 GetAxis2D(Common2DAxis axis)
		{
			if (axis == Common2DAxis.none)
			{
				return Vector2.zero;
			}
			if (device.TryGetFeatureValue(GetCommon2DAxis(axis), out var value))
			{
				return value;
			}
			return Vector2.zero;
		}

		public override void TryHapticImpulse(float duration, float amp, float freq = 0f)
		{
			foreach (InputDevice item in Devices())
			{
				if (item.TryGetHapticCapabilities(out var capabilities) && capabilities.supportsImpulse)
				{
					item.SendHapticImpulse(0u, amp, duration);
				}
			}
		}

		public static InputFeatureUsage<bool> GetCommonButton(CommonButton button)
		{
			return button switch
			{
				CommonButton.gripButton => CommonUsages.gripButton, 
				CommonButton.menuButton => CommonUsages.menuButton, 
				CommonButton.primary2DAxisClick => CommonUsages.primary2DAxisClick, 
				CommonButton.primary2DAxisTouch => CommonUsages.primary2DAxisTouch, 
				CommonButton.primaryButton => CommonUsages.primaryButton, 
				CommonButton.primaryTouch => CommonUsages.primaryTouch, 
				CommonButton.secondary2DAxisClick => CommonUsages.secondary2DAxisClick, 
				CommonButton.secondary2DAxisTouch => CommonUsages.secondary2DAxisTouch, 
				CommonButton.secondaryButton => CommonUsages.secondaryButton, 
				CommonButton.secondaryTouch => CommonUsages.secondaryTouch, 
				_ => CommonUsages.triggerButton, 
			};
		}

		public static InputFeatureUsage<float> GetCommonAxis(CommonAxis axis)
		{
			if (axis == CommonAxis.grip)
			{
				return CommonUsages.grip;
			}
			return CommonUsages.trigger;
		}

		public static InputFeatureUsage<Vector2> GetCommon2DAxis(Common2DAxis axis)
		{
			if (axis == Common2DAxis.primaryAxis)
			{
				return CommonUsages.primary2DAxis;
			}
			return CommonUsages.secondary2DAxis;
		}
	}
}
