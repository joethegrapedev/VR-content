using System;
using UnityEngine;

[Serializable]
public struct DeviceData
{
	public string[] deviceNames;

	public Vector3 position;

	public Vector3 rotation;

	public DeviceData(string name, Vector3 pos, Vector3 rot)
	{
		deviceNames = new string[1] { name };
		position = pos;
		rotation = rot;
	}

	public DeviceData(string[] names, Vector3 pos, Vector3 rot)
	{
		deviceNames = names;
		position = pos;
		rotation = rot;
	}
}
