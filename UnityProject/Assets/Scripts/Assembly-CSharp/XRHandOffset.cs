using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class XRHandOffset : MonoBehaviour
{
	[Tooltip("DO NOT CHANGE THIS UNLESS YOU ARE REDOING THE RELATIVE POSITIONS. This is the device that you are using to setup the innital proper orientation of the hand, all offsets are relative to this device")]
	public string defaultDevice = "Oculus";

	[SerializeField]
	public Transform[] rightOffsets;

	[SerializeField]
	public Transform[] leftOffsets;

	[SerializeField]
	public DeviceData[] devices = new DeviceData[3]
	{
		new DeviceData("Oculus", new Vector3(0.005f, -0.016f, 0.014f), new Vector3(48f, 0f, 15f)),
		new DeviceData("Windows MR", new Vector3(0.003f, -0.005f, -0.078f), new Vector3(36f, -12f, 2f)),
		new DeviceData(new string[5] { "Vive", "HTC", "Index", "Cosmos", "Elite" }, new Vector3(0.015f, 0f, 0.0412f), new Vector3(30f, -17f, 0f))
	};

	private bool offsetDone;

	private bool hasProvider;

	private void OnEnable()
	{
		InputDevices.deviceConnected += DeviceConnected;
		List<InputDevice> list = new List<InputDevice>();
		InputDevices.GetDevices(list);
		foreach (InputDevice item in list)
		{
			DeviceConnected(item);
		}
	}

	private void OnDisable()
	{
		if (hasProvider)
		{
			InputDevices.deviceConnected -= DeviceConnected;
		}
	}

	internal void AdjustPositions(XRHandOffset otherOffset)
	{
		Vector3 vector = GetDefaultPositionOffset() - otherOffset.GetDefaultPositionOffset();
		Vector3 vector2 = GetDefaultRotationOffset() - otherOffset.GetDefaultRotationOffset();
		Transform[] array = leftOffsets;
		foreach (Transform obj in array)
		{
			obj.localPosition += new Vector3(0f - vector.x, vector.y, vector.z);
			obj.localEulerAngles += new Vector3(vector2.x, 0f - vector2.y, 0f - vector2.z);
		}
		array = rightOffsets;
		foreach (Transform obj2 in array)
		{
			obj2.localPosition += vector;
			obj2.localEulerAngles += vector2;
		}
	}

	private void DeviceConnected(InputDevice inputDevice)
	{
		if (inputDevice.characteristics == InputDeviceCharacteristics.None)
		{
			return;
		}
		DeviceData[] array = devices;
		for (int i = 0; i < array.Length; i++)
		{
			DeviceData deviceData = array[i];
			if (offsetDone)
			{
				break;
			}
			for (int j = 0; j < deviceData.deviceNames.Length; j++)
			{
				if (inputDevice.name.Contains(deviceData.deviceNames[j]))
				{
					Vector3 positionOffset = GetPositionOffset(defaultDevice, deviceData.deviceNames[j]);
					Vector3 rotationOffset = GetRotationOffset(defaultDevice, deviceData.deviceNames[j]);
					Transform[] array2 = leftOffsets;
					foreach (Transform obj in array2)
					{
						obj.localPosition += new Vector3(0f - positionOffset.x, positionOffset.y, positionOffset.z);
						obj.localEulerAngles += new Vector3(rotationOffset.x, 0f - rotationOffset.y, 0f - rotationOffset.z);
					}
					array2 = rightOffsets;
					foreach (Transform obj2 in array2)
					{
						obj2.localPosition += positionOffset;
						obj2.localEulerAngles += rotationOffset;
					}
					OnDisable();
					offsetDone = true;
					break;
				}
			}
		}
	}

	private Vector3 GetPositionOffset(string from, string to)
	{
		if (from == to)
		{
			return Vector3.zero;
		}
		Vector3 vector2;
		Vector3 vector = (vector2 = Vector3.zero);
		DeviceData[] array = devices;
		for (int i = 0; i < array.Length; i++)
		{
			DeviceData deviceData = array[i];
			string[] deviceNames = deviceData.deviceNames;
			foreach (string obj in deviceNames)
			{
				if (obj == from)
				{
					vector2 = deviceData.position;
				}
				if (obj == to)
				{
					vector = deviceData.position;
				}
			}
		}
		return vector - vector2;
	}

	private Vector3 GetRotationOffset(string from, string to)
	{
		if (from == to)
		{
			return Vector3.zero;
		}
		Vector3 vector2;
		Vector3 vector = (vector2 = Vector3.zero);
		DeviceData[] array = devices;
		for (int i = 0; i < array.Length; i++)
		{
			DeviceData deviceData = array[i];
			string[] deviceNames = deviceData.deviceNames;
			foreach (string obj in deviceNames)
			{
				if (obj == from)
				{
					vector2 = deviceData.rotation;
				}
				if (obj == to)
				{
					vector = deviceData.rotation;
				}
			}
		}
		return vector - vector2;
	}

	protected Vector3 GetDefaultPositionOffset()
	{
		Vector3 result = Vector3.zero;
		DeviceData[] array = devices;
		for (int i = 0; i < array.Length; i++)
		{
			DeviceData deviceData = array[i];
			string[] deviceNames = deviceData.deviceNames;
			for (int j = 0; j < deviceNames.Length; j++)
			{
				if (deviceNames[j] == defaultDevice)
				{
					result = deviceData.position;
				}
			}
		}
		return result;
	}

	protected Vector3 GetDefaultRotationOffset()
	{
		Vector3 result = Vector3.zero;
		DeviceData[] array = devices;
		for (int i = 0; i < array.Length; i++)
		{
			DeviceData deviceData = array[i];
			string[] deviceNames = deviceData.deviceNames;
			for (int j = 0; j < deviceNames.Length; j++)
			{
				if (deviceNames[j] == defaultDevice)
				{
					result = deviceData.rotation;
				}
			}
		}
		return result;
	}
}
