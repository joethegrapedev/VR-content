using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class OpenXRControllerEvent : MonoBehaviour
{
	public InputActionProperty action;

	public UnityEvent inputEvent;

	private void OnEnable()
	{
		action.action.Enable();
		action.action.performed += delegate
		{
			inputEvent?.Invoke();
		};
	}

	private void OnDisable()
	{
		action.action.performed -= delegate
		{
			inputEvent?.Invoke();
		};
	}
}
