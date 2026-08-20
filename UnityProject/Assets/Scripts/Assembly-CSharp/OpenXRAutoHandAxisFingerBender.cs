using Autohand;
using UnityEngine;
using UnityEngine.InputSystem;

public class OpenXRAutoHandAxisFingerBender : MonoBehaviour
{
	public Hand hand;

	public InputActionProperty bendAction;

	[HideInInspector]
	public float[] bendOffsets;

	private float lastAxis;

	private void LateUpdate()
	{
		float num = bendAction.action.ReadValue<float>();
		for (int i = 0; i < bendOffsets.Length; i++)
		{
			hand.fingers[i].bendOffset += (num - lastAxis) * bendOffsets[i];
		}
		lastAxis = num;
	}
}
