using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stops the narration playing for a task option panel.
///
/// The recovered original searched only <c>GetComponentsInParent</c>, which
/// misses any AudioSource sitting on a child of the canvas. Both directions
/// are searched here so closing a panel reliably silences its narration
/// instead of letting clips overlap.
/// </summary>
public class AudioStop : MonoBehaviour
{
	public GameObject optionCanvas;

	public void StopAudio()
	{
		if (optionCanvas == null)
		{
			return;
		}

		HashSet<AudioSource> stopped = new HashSet<AudioSource>();
		StopAll(optionCanvas.GetComponentsInParent<AudioSource>(true), stopped);
		StopAll(optionCanvas.GetComponentsInChildren<AudioSource>(true), stopped);
	}

	private static void StopAll(AudioSource[] sources, HashSet<AudioSource> stopped)
	{
		if (sources == null)
		{
			return;
		}
		for (int i = 0; i < sources.Length; i++)
		{
			AudioSource source = sources[i];
			if (source != null && stopped.Add(source))
			{
				source.Stop();
			}
		}
	}
}
