using UnityEngine;
using UnityEngine.XR.Management;

[DefaultExecutionOrder(-10000)]
public class XRProviderPicker : MonoBehaviour
{
	public string providerName = "";

	public XRHandOffset enableMe;

	public XRHandOffset disableMe;

	private bool hasProvider;

	private void Start()
	{
		foreach (XRLoader activeLoader in XRGeneralSettings.Instance.Manager.activeLoaders)
		{
			if (providerName == "" || providerName == activeLoader.name)
			{
				hasProvider = true;
			}
		}
		if (hasProvider)
		{
			enableMe.enabled = true;
			disableMe.enabled = false;
		}
		else
		{
			disableMe.AdjustPositions(enableMe);
			enableMe.enabled = false;
			disableMe.enabled = true;
		}
	}
}
