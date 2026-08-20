using System.Collections;
using UnityEngine;

public class PlatformMover : MonoBehaviour
{
	public Vector3 toRange;

	public float time = 1f;

	private Vector3 startPos;

	private void Start()
	{
		startPos = base.transform.position;
		StartCoroutine(Move());
	}

	private IEnumerator Move()
	{
		while (true)
		{
			float timePassed = 0f;
			while (timePassed < time)
			{
				timePassed += Time.fixedDeltaTime;
				base.transform.position = Vector3.Lerp(startPos, startPos + toRange, timePassed / time);
				yield return new WaitForEndOfFrame();
			}
			base.transform.position = toRange;
			timePassed = 0f;
			while (timePassed < time)
			{
				timePassed += Time.fixedDeltaTime;
				base.transform.position = Vector3.Lerp(startPos + toRange, startPos, timePassed / time);
				yield return new WaitForEndOfFrame();
			}
			base.transform.position = toRange;
		}
	}
}
