using UnityEngine;

public class OpenLink : MonoBehaviour
{
	public string URL;

	public void OpenLinkURL()
	{
		Application.OpenURL(URL);
	}
}
