using UnityEngine;
using UnityEngine.InputSystem.UI;

namespace Michsky.MUIP
{
	public class InputSystemChecker : MonoBehaviour
	{
		private void Awake()
		{
			if (base.gameObject.GetComponent<InputSystemUIInputModule>() == null)
			{
				Debug.LogError("<b>[Modern UI Pack]</b> Input System is enabled, but <b>'Input System UI Input Module'</b> is missing. Select the event system object, and click the <b>'Replace'</b> button.");
			}
		}
	}
}
