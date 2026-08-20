using UnityEngine;

public class FaceCamera : MonoBehaviour
{
	public Transform mainCamera;

	private void LateUpdate()
	{
		base.transform.LookAt(base.transform.position + mainCamera.forward);
	}
}
