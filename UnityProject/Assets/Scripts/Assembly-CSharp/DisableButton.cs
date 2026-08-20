using UnityEngine;
using UnityEngine.UI;

public class DisableButton : MonoBehaviour
{
	private RectTransform crossImageRectTransform;

	public Image crossImage;

	public GameObject wrongButton;

	private void Start()
	{
		crossImageRectTransform = crossImage.GetComponent<RectTransform>();
	}

	public void DisableWrongButton()
	{
		LeanTween.alpha(crossImageRectTransform, 1f, 1f);
		wrongButton.SetActive(value: false);
	}
}
