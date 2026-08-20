using TMPro;
using UnityEngine;

public class PlayerScore : MonoBehaviour
{
	public TextMeshProUGUI scoreUI;

	public int Total;

	public void AddTotal(int points)
	{
		Total += points;
		scoreUI.text = "Score: " + Total;
	}
}
