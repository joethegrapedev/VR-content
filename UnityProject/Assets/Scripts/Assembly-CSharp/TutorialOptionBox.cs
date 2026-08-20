using UnityEngine;
using UnityEngine.UI;

public class TutorialOptionBox : MonoBehaviour
{
	public GameObject taskObject;

	public GameObject interactionMenu;

	public Image checkMarker;

	private RectTransform checkMarkerRectTransform;

	private void Start()
	{
		checkMarkerRectTransform = checkMarker.GetComponent<RectTransform>();
	}

	public void CorrectOption()
	{
		LeanTween.alpha(checkMarkerRectTransform, 1f, 1f);
		interactionMenu.SetActive(value: false);
	}
}
