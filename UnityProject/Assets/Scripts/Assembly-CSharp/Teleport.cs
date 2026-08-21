using System.Collections;
using UnityEngine;

public class Teleport : MonoBehaviour
{
	[SerializeField]
	private Transform teleport;

	[SerializeField]
	private GameObject player;

	private bool isEnabled;

	private void Start()
	{
		isEnabled = true;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (isEnabled)
		{
			StartCoroutine(Tp());
		}
	}

	private IEnumerator Tp()
	{
		yield return new WaitForSeconds(1f);
		player.transform.position = new Vector3(teleport.transform.position.x, teleport.transform.position.y, teleport.transform.position.z);
		GameAudioCues.Teleport(player.transform.position);
	}
}
