using System.Collections;
using TMPro;
using UnityEngine;

namespace Autohand.Demo
{
	public class TextChanger : MonoBehaviour
	{
		public TextMeshPro text;

		private Coroutine changing;

		private Coroutine hide;

		public void UpdateText(string newText, float upTime)
		{
		}

		public void UpdateText(string newText)
		{
		}

		private IEnumerator ChangeText(float seconds, string newText)
		{
			yield return new WaitForFixedUpdate();
			text.text = "";
		}

		private void OnDestroy()
		{
			text.text = "";
		}
	}
}
