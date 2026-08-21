using TMPro;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
	public GameObject endButton;

	public MyRuntimeTest RunTimescript;

	public TextMeshProUGUI TMP_Text;

	private bool pauseActive;

	private bool ended;

	private void Start()
	{
		endButton.SetActive(value: false);
	}

	public void DisplayWristUI()
	{
		if (pauseActive)
		{
			Time.timeScale = 1f;
			AudioListener.pause = false;
			pauseActive = false;
			if (!ended)
			{
				endButton.SetActive(value: false);
			}
		}
		else if (!pauseActive)
		{
			Time.timeScale = 0f;
			AudioListener.pause = true;
			pauseActive = true;
			if (!ended)
			{
				endButton.SetActive(value: true);
			}
		}
	}

	public void EndGame()
	{
		Time.timeScale = 1f;
		AudioListener.pause = false;
		RunTimescript.finished = false;
		RunTimescript.UpdateExcel();
		RunTimescript.countdown = TimerSettings.setTime;
		ended = true;
		endButton.SetActive(value: false);
		TMP_Text.text = "DNF";
	}
}
