using UnityEngine;

namespace Autohand.Demo
{
	public class DemoFlyingToggle : MonoBehaviour
	{
		public void ToggleFlying()
		{
			AutoHandPlayer.Instance.ToggleFlying();
		}
	}
}
