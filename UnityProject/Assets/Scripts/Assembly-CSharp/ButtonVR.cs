using UnityEngine;
using UnityEngine.Events;

public class ButtonVR : MonoBehaviour
{
	public GameObject button;

	public UnityEvent onPress;

	public UnityEvent onRelease;

	private GameObject presser;

	private AudioSource sound;

	private bool isPressed;

	private void Start()
	{
		sound = GetComponent<AudioSource>();
		isPressed = false;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!isPressed)
		{
			button.transform.localPosition = new Vector3(0f, 0.003f, 0f);
			presser = other.gameObject;
			onPress.Invoke();
			sound.Play();
			isPressed = true;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject == presser)
		{
			button.transform.localPosition = new Vector3(0f, 0.015f, 0f);
			onRelease.Invoke();
			isPressed = false;
		}
	}

	public void SpawnSphere()
	{
		GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		obj.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
		obj.transform.localPosition = new Vector3(0f, 1f, 2f);
		obj.AddComponent<Rigidbody>();
	}
}
