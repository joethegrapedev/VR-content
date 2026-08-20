using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;

public class TypingArea : XRGrabInteractable
{
	public Keyboards keyboardScript;

	public TMP_Text key;

	public TMP_Text inputField;

	public TMP_Text warningText;

	private string savePath = "/_MyProject/saves";

	protected override void Grab()
	{
		base.Grab();
		switch (key.text)
		{
		case "Enter":
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
			break;
		case "Caps":
			keyboardScript.CapsPressed();
			break;
		case "Delete":
			keyboardScript.DeleteChar();
			break;
		case "":
			keyboardScript.InsertSpace();
			break;
		case "1":
			keyboardScript.InsertChar("1");
			break;
		case "2":
			keyboardScript.InsertChar("2");
			break;
		case "3":
			keyboardScript.InsertChar("3");
			break;
		case "4":
			keyboardScript.InsertChar("4");
			break;
		case "5":
			keyboardScript.InsertChar("5");
			break;
		case "6":
			keyboardScript.InsertChar("6");
			break;
		case "7":
			keyboardScript.InsertChar("7");
			break;
		case "8":
			keyboardScript.InsertChar("8");
			break;
		case "9":
			keyboardScript.InsertChar("9");
			break;
		case "0":
			keyboardScript.InsertChar("0");
			break;
		case "a":
			keyboardScript.InsertChar("a");
			break;
		case "b":
			keyboardScript.InsertChar("b");
			break;
		case "c":
			keyboardScript.InsertChar("c");
			break;
		case "d":
			keyboardScript.InsertChar("d");
			break;
		case "e":
			keyboardScript.InsertChar("e");
			break;
		case "f":
			keyboardScript.InsertChar("f");
			break;
		case "g":
			keyboardScript.InsertChar("g");
			break;
		case "h":
			keyboardScript.InsertChar("h");
			break;
		case "i":
			keyboardScript.InsertChar("i");
			break;
		case "j":
			keyboardScript.InsertChar("j");
			break;
		case "k":
			keyboardScript.InsertChar("k");
			break;
		case "l":
			keyboardScript.InsertChar("l");
			break;
		case "m":
			keyboardScript.InsertChar("m");
			break;
		case "n":
			keyboardScript.InsertChar("n");
			break;
		case "o":
			keyboardScript.InsertChar("o");
			break;
		case "p":
			keyboardScript.InsertChar("p");
			break;
		case "q":
			keyboardScript.InsertChar("q");
			break;
		case "r":
			keyboardScript.InsertChar("r");
			break;
		case "s":
			keyboardScript.InsertChar("s");
			break;
		case "t":
			keyboardScript.InsertChar("t");
			break;
		case "u":
			keyboardScript.InsertChar("u");
			break;
		case "v":
			keyboardScript.InsertChar("v");
			break;
		case "w":
			keyboardScript.InsertChar("w");
			break;
		case "x":
			keyboardScript.InsertChar("x");
			break;
		case "y":
			keyboardScript.InsertChar("y");
			break;
		case "z":
			keyboardScript.InsertChar("z");
			break;
		case "A":
			keyboardScript.InsertChar("A");
			break;
		case "B":
			keyboardScript.InsertChar("B");
			break;
		case "C":
			keyboardScript.InsertChar("C");
			break;
		case "D":
			keyboardScript.InsertChar("D");
			break;
		case "E":
			keyboardScript.InsertChar("E");
			break;
		case "F":
			keyboardScript.InsertChar("F");
			break;
		case "G":
			keyboardScript.InsertChar("G");
			break;
		case "H":
			keyboardScript.InsertChar("H");
			break;
		case "I":
			keyboardScript.InsertChar("I");
			break;
		case "J":
			keyboardScript.InsertChar("J");
			break;
		case "K":
			keyboardScript.InsertChar("K");
			break;
		case "L":
			keyboardScript.InsertChar("L");
			break;
		case "M":
			keyboardScript.InsertChar("M");
			break;
		case "N":
			keyboardScript.InsertChar("N");
			break;
		case "O":
			keyboardScript.InsertChar("O");
			break;
		case "P":
			keyboardScript.InsertChar("P");
			break;
		case "Q":
			keyboardScript.InsertChar("Q");
			break;
		case "R":
			keyboardScript.InsertChar("R");
			break;
		case "S":
			keyboardScript.InsertChar("S");
			break;
		case "T":
			keyboardScript.InsertChar("T");
			break;
		case "U":
			keyboardScript.InsertChar("U");
			break;
		case "V":
			keyboardScript.InsertChar("V");
			break;
		case "W":
			keyboardScript.InsertChar("W");
			break;
		case "X":
			keyboardScript.InsertChar("X");
			break;
		case "Y":
			keyboardScript.InsertChar("Y");
			break;
		case "Z":
			keyboardScript.InsertChar("Z");
			break;
		}
	}
}
