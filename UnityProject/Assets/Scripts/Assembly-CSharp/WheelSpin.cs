using UnityEngine;

public class WheelSpin : MonoBehaviour
{
	public Vector3 _rotation;

	private void Update()
	{
		base.transform.Rotate(_rotation * Time.deltaTime);
	}
}
