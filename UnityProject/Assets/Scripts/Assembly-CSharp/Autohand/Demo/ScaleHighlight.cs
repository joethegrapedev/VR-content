using UnityEngine;

namespace Autohand.Demo
{
	public class ScaleHighlight : MonoBehaviour
	{
		public Vector3 highlighScale;

		public Vector3 normalScale;

		public void Highlight()
		{
			base.transform.localScale = highlighScale;
		}

		public void HighlightStop()
		{
			base.transform.localScale = normalScale;
		}
	}
}
