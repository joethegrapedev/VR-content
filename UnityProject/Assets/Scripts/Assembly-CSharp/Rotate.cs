using UnityEngine;

public class Rotate : MonoBehaviour
{
	public GameObject checkMarker;

	private void Start()
	{
		LeanTween.rotateAround(checkMarker, Vector3.up, 360f, 5f).setLoopClamp();
	}
}
