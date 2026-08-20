using UnityEngine;

public class HeadCameraSmoothing : MonoBehaviour
{
	public Vector3 lastPos;

	public Quaternion lastRot;

	public void LateUpdate()
	{
		lastPos = base.transform.position;
		lastRot = base.transform.rotation;
	}
}
