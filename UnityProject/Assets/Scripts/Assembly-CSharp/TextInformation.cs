using UnityEngine;

public class TextInformation : MonoBehaviour
{
	public GameObject activateImage;

	public GameObject deactivateImage;

	public GameObject[] texts;

	private bool active;

	public void ActivateText()
	{
		active = true;
		GameObject[] array = texts;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(active);
		}
		activateImage.SetActive(value: true);
		deactivateImage.SetActive(value: false);
	}

	public void DeactivateText()
	{
		active = false;
		GameObject[] array = texts;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(active);
		}
		activateImage.SetActive(value: false);
		deactivateImage.SetActive(value: true);
	}

	public void ToggleText()
	{
		if (active)
		{
			DeactivateText();
		}
		else
		{
			ActivateText();
		}
	}
}
