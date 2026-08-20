using UnityEngine;

namespace Autohand
{
	public class EditorHand : MonoBehaviour
	{
		public bool useEditorGUI = true;

		public GrabbablePose grabbablePose;

		public HandPoseArea grabbablePoseArea;

		private Hand _hand;

		public Hand hand
		{
			get
			{
				if (_hand == null)
				{
					_hand = GetComponent<Hand>();
				}
				return _hand;
			}
		}
	}
}
