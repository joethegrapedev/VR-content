using TMPro;
using UnityEngine;

public class TMPSizeProtector : MonoBehaviour
{
	public float size;

	private void Start()
	{
		if (GetComponent<TextMeshPro>() != null)
		{
			GetComponent<TextMeshPro>().fontSize = size;
		}
	}
}
