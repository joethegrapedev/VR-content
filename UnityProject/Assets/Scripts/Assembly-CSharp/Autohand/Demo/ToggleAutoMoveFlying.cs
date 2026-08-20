using UnityEngine;

namespace Autohand.Demo
{
	public class ToggleAutoMoveFlying : MonoBehaviour
	{
		public void ToggleFlying()
		{
			AutoHandPlayer autoHandPlayer = Object.FindObjectOfType<AutoHandPlayer>();
			autoHandPlayer.useGrounding = !autoHandPlayer.useGrounding;
		}
	}
}
