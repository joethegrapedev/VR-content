using TMPro;
using UnityEngine;

public class Keyboards : MonoBehaviour
{
	public TMP_InputField inputField;

	public GameObject normalButtons;

	public GameObject capsButtons;

	public bool caps;

	private void Start()
	{
		caps = false;
	}

	public void InsertChar(string c)
	{
		inputField.text += c;
	}

	public void DeleteChar()
	{
		if (inputField.text.Length > 0)
		{
			inputField.text = inputField.text.Substring(0, inputField.text.Length - 1);
		}
	}

	public void InsertSpace()
	{
		inputField.text += " ";
	}

	public void CapsPressed()
	{
		if (!caps)
		{
			normalButtons.SetActive(value: false);
			capsButtons.SetActive(value: true);
			caps = true;
		}
		else
		{
			capsButtons.SetActive(value: false);
			normalButtons.SetActive(value: true);
			caps = false;
		}
	}
}
