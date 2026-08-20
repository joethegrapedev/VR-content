using System.Collections;
using System.IO;
using UnityEngine;

public class LargeScreenShot : MonoBehaviour
{
	private Texture2D ScreenShot;

	private RenderTexture rt;

	private int res = 2560;

	private int res2 = 1440;

	private void Start()
	{
		rt = new RenderTexture(res, res2, 32);
		ScreenShot = new Texture2D(res, res2, TextureFormat.RGBA32, mipChain: false);
		Camera.main.targetTexture = rt;
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.P))
		{
			Camera.main.Render();
			RenderTexture.active = rt;
			ScreenShot.ReadPixels(new Rect(0f, 0f, res, res2), 0, 0);
			ScreenShot.Apply();
			byte[] bytes = ScreenShot.EncodeToPNG();
			Object.Destroy(ScreenShot);
			File.WriteAllBytes(System.IO.Path.Combine(Application.persistentDataPath, "SavedScreen.png"), bytes);
		}
	}

	private IEnumerator take()
	{
		yield return new WaitForEndOfFrame();
	}
}
