using System.Collections;
using UnityEngine;

public class FlickerLight : MonoBehaviour
{
	public bool isFlickering;

	public float timeDelay;

	private void Update()
	{
		if (!isFlickering)
		{
			StartCoroutine(FlickeringLight());
		}
	}

	private IEnumerator FlickeringLight()
	{
		isFlickering = true;
		base.gameObject.GetComponent<Light>().enabled = false;
		timeDelay = Random.Range(0.01f, 0.2f);
		yield return new WaitForSeconds(timeDelay);
		base.gameObject.GetComponent<Light>().enabled = true;
		timeDelay = Random.Range(0.01f, 0.2f);
		yield return new WaitForSeconds(timeDelay);
		isFlickering = false;
	}
}
