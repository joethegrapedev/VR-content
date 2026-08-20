using TMPro;
using UnityEngine;

public class KeyboardButton : MonoBehaviour
{
	private Keyboards keyboard;

	private TextMeshProUGUI buttonText;

	private void Start()
	{
		keyboard = GetComponentInParent<Keyboards>();
		buttonText = GetComponentInChildren<TextMeshProUGUI>();
		if (buttonText.text.Length == 1)
		{
			NameToButtonText();
			GetComponentInChildren<ButtonVR>().onRelease.AddListener(delegate
			{
				keyboard.InsertChar(buttonText.text);
			});
		}
	}

	public void NameToButtonText()
	{
		buttonText.text = base.gameObject.name;
	}
}
