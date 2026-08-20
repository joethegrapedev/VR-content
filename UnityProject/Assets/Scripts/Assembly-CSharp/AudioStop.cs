using UnityEngine;

public class AudioStop : MonoBehaviour
{
	public GameObject optionCanvas;

	public void StopAudio()
	{
		Component[] componentsInParent = optionCanvas.GetComponentsInParent(typeof(AudioSource));
		for (int i = 0; i < componentsInParent.Length; i++)
		{
			((AudioSource)componentsInParent[i]).Stop();
		}
	}
}
