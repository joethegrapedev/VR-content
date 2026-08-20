using UnityEngine;

[DefaultExecutionOrder(-100000)]
public class IgnoreHandLayer : MonoBehaviour
{
	public bool includeChildren = true;

	private int startLayer;

	private void Awake()
	{
		startLayer = base.gameObject.layer;
		Invoke("LateStart", 0.1f);
	}

	private void LateStart()
	{
		if (includeChildren)
		{
			SetLayerRecursive(base.transform, startLayer);
		}
		else
		{
			base.transform.gameObject.layer = startLayer;
		}
	}

	internal void SetLayerRecursive(Transform obj, int newLayer)
	{
		obj.gameObject.layer = newLayer;
		for (int i = 0; i < obj.childCount; i++)
		{
			SetLayerRecursive(obj.GetChild(i), newLayer);
		}
	}
}
