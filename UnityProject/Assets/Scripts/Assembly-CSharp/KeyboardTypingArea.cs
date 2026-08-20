using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class KeyboardTypingArea : MonoBehaviour
{
	public Keyboards keyboardScript;

	public TMP_Text inputField;

	public TMP_Text warningText;

	private Myproject playerControls;

	private InputAction inputs;

	private string savePath = "/_MyProject/saves";

	private void Awake()
	{
		playerControls = new Myproject();
	}

	private void Start()
	{
	}

	private void Update()
	{
	}

	private void OnEnable()
	{
		inputs = playerControls.Keyboard.Keys;
		inputs.Enable();
		inputs.performed += Keys;
	}

	private void OnDisable()
	{
		inputs.Disable();
	}

	private void Keys(InputAction.CallbackContext context)
	{
		float num = context.ReadValue<float>();
		if (num <= 20f)
		{
			if (num <= 10f)
			{
				if (num <= 5f)
				{
					if (num <= 2f)
					{
						if (num != 1f)
						{
							if (num == 2f)
							{
								keyboardScript.DeleteChar();
							}
						}
						else
						{
							keyboardScript.CapsPressed();
						}
					}
					else if (num != 3f)
					{
						if (num != 4f)
						{
							if (num == 5f)
							{
								keyboardScript.InsertChar(Caps("w"));
							}
						}
						else
						{
							keyboardScript.InsertChar(Caps("q"));
						}
					}
					else
					{
						keyboardScript.InsertSpace();
					}
				}
				else if (num <= 7f)
				{
					if (num != 6f)
					{
						if (num == 7f)
						{
							keyboardScript.InsertChar(Caps("r"));
						}
					}
					else
					{
						keyboardScript.InsertChar(Caps("e"));
					}
				}
				else if (num != 8f)
				{
					if (num != 9f)
					{
						if (num == 10f)
						{
							keyboardScript.InsertChar(Caps("u"));
						}
					}
					else
					{
						keyboardScript.InsertChar(Caps("y"));
					}
				}
				else
				{
					keyboardScript.InsertChar(Caps("t"));
				}
			}
			else if (num <= 15f)
			{
				if (num <= 12f)
				{
					if (num != 11f)
					{
						if (num == 12f)
						{
							keyboardScript.InsertChar(Caps("o"));
						}
					}
					else
					{
						keyboardScript.InsertChar(Caps("i"));
					}
				}
				else if (num != 13f)
				{
					if (num != 14f)
					{
						if (num == 15f)
						{
							keyboardScript.InsertChar(Caps("s"));
						}
					}
					else
					{
						keyboardScript.InsertChar(Caps("a"));
					}
				}
				else
				{
					keyboardScript.InsertChar(Caps("p"));
				}
			}
			else if (num <= 17f)
			{
				if (num != 16f)
				{
					if (num == 17f)
					{
						keyboardScript.InsertChar(Caps("f"));
					}
				}
				else
				{
					keyboardScript.InsertChar(Caps("d"));
				}
			}
			else if (num != 18f)
			{
				if (num != 19f)
				{
					if (num == 20f)
					{
						keyboardScript.InsertChar(Caps("j"));
					}
				}
				else
				{
					keyboardScript.InsertChar(Caps("h"));
				}
			}
			else
			{
				keyboardScript.InsertChar(Caps("g"));
			}
		}
		else if (num <= 30f)
		{
			if (num <= 25f)
			{
				if (num <= 22f)
				{
					if (num != 21f)
					{
						if (num == 22f)
						{
							keyboardScript.InsertChar(Caps("l"));
						}
					}
					else
					{
						keyboardScript.InsertChar(Caps("k"));
					}
				}
				else if (num != 23f)
				{
					if (num != 24f)
					{
						if (num == 25f)
						{
							keyboardScript.InsertChar(Caps("c"));
						}
					}
					else
					{
						keyboardScript.InsertChar(Caps("x"));
					}
				}
				else
				{
					keyboardScript.InsertChar(Caps("z"));
				}
			}
			else if (num <= 27f)
			{
				if (num != 26f)
				{
					if (num == 27f)
					{
						keyboardScript.InsertChar(Caps("b"));
					}
				}
				else
				{
					keyboardScript.InsertChar(Caps("v"));
				}
			}
			else if (num != 28f)
			{
				if (num != 29f)
				{
					if (num == 30f)
					{
						keyboardScript.InsertChar("1");
					}
				}
				else
				{
					keyboardScript.InsertChar(Caps("m"));
				}
			}
			else
			{
				keyboardScript.InsertChar(Caps("n"));
			}
		}
		else if (num <= 35f)
		{
			if (num <= 32f)
			{
				if (num != 31f)
				{
					if (num == 32f)
					{
						keyboardScript.InsertChar("3");
					}
				}
				else
				{
					keyboardScript.InsertChar("2");
				}
			}
			else if (num != 33f)
			{
				if (num != 34f)
				{
					if (num == 35f)
					{
						keyboardScript.InsertChar("6");
					}
				}
				else
				{
					keyboardScript.InsertChar("5");
				}
			}
			else
			{
				keyboardScript.InsertChar("4");
			}
		}
		else if (num <= 37f)
		{
			if (num != 36f)
			{
				if (num == 37f)
				{
					keyboardScript.InsertChar("8");
				}
			}
			else
			{
				keyboardScript.InsertChar("7");
			}
		}
		else if (num != 38f)
		{
			if (num != 39f)
			{
				if (num != 40f)
				{
					return;
				}
				if (inputField.text.Length > 2)
				{
					if (!Directory.Exists(SavePaths.SavesDirectory))
					{
						Directory.CreateDirectory(SavePaths.SavesDirectory);
					}
					string path = SavePaths.PlayerInfoFile;
					string contents = JsonUtility.ToJson(new PlayerName
					{
						Player = inputField.text
					}, prettyPrint: true);
					File.WriteAllText(path, contents);
					SceneManager.LoadScene("5SD Map");
				}
				else
				{
					warningText.text = "Name must be at least 2 characters long!";
				}
			}
			else
			{
				keyboardScript.InsertChar("0");
			}
		}
		else
		{
			keyboardScript.InsertChar("9");
		}
	}

	private string Caps(string letter)
	{
		if (keyboardScript.caps)
		{
			return letter.ToUpper();
		}
		return letter;
	}
}
