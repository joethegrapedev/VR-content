using UnityEngine;
using UnityEngine.SceneManagement;

namespace Autohand.Demo
{
	public class PlayerFallCatcher : MonoBehaviour
	{
		private Vector3 startPos;

		private void Start()
		{
			if (AutoHandPlayer.Instance != null)
			{
				startPos = AutoHandPlayer.Instance.transform.position;
				if (!SceneManager.GetActiveScene().name.ToLower().Contains("demo"))
				{
					base.enabled = false;
				}
			}
		}

		private void Update()
		{
			if (AutoHandPlayer.Instance != null && AutoHandPlayer.Instance.transform.position.y < -10f)
			{
				AutoHandPlayer.Instance.SetPosition(startPos);
			}
		}
	}
}
