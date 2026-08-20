using System.IO;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AudioSettings : MonoBehaviour
{
	[SerializeField]
	private AudioMixer myAudioMixer;

	private Audio settings = new Audio();

	private string path;

	private string savePath = "/_MyProject/saves";

	public Slider Dialogue;

	public Slider SFX;

	protected void Start()
	{
		if (!Directory.Exists(SavePaths.SavesDirectory))
		{
			Directory.CreateDirectory(SavePaths.SavesDirectory);
		}
		path = SavePaths.AudioSettingsFile;
		if (SceneManager.GetActiveScene().name == "Menu" || SceneManager.GetActiveScene().name == "5SD Map")
		{
			if (!File.Exists(path))
			{
				myAudioMixer.SetFloat("Dialogue", -6f);
				myAudioMixer.SetFloat("SFX", -6f);
			}
			else if (File.Exists(path))
			{
				myAudioMixer.SetFloat("Dialogue", Mathf.Log10(JsonUtility.FromJson<Audio>(File.ReadAllText(path)).Volume[0]) * 20f);
				myAudioMixer.SetFloat("SFX", Mathf.Log10(JsonUtility.FromJson<Audio>(File.ReadAllText(path)).Volume[1]) * 20f);
			}
		}
		else if (SceneManager.GetActiveScene().name == "Audio Settings")
		{
			if (!File.Exists(path))
			{
				myAudioMixer.SetFloat("Dialogue", -6f);
				myAudioMixer.SetFloat("SFX", -6f);
			}
			else if (File.Exists(path))
			{
				Dialogue.value = JsonUtility.FromJson<Audio>(File.ReadAllText(path)).Volume[0];
				SFX.value = JsonUtility.FromJson<Audio>(File.ReadAllText(path)).Volume[1];
			}
		}
	}

	public void UpdateDialogue(float sliderValue)
	{
		myAudioMixer.SetFloat("Dialogue", Mathf.Log10(sliderValue) * 20f);
	}

	public void UpdateSFX(float sliderValue)
	{
		myAudioMixer.SetFloat("SFX", Mathf.Log10(sliderValue) * 20f);
	}

	public void SaveSettings()
	{
		settings.Volume.Add(Dialogue.value);
		settings.Volume.Add(SFX.value);
		string contents = JsonUtility.ToJson(settings, prettyPrint: true);
		File.WriteAllText(path, contents);
	}
}
