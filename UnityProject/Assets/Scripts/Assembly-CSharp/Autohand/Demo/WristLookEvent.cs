using UnityEngine;

namespace Autohand.Demo
{
	public class WristLookEvent : MonoBehaviour
	{
		public Hand hand;

		public Camera head;

		[Tooltip("The minimum head->wrist distance required to activate")]
		public float maxDistance = 0.75f;

		[Tooltip("The angle precisness required to activate; 0 is any angle, 1 is exactly pointed at the face")]
		[Range(0f, 1f)]
		public float anglePreciseness = 0.75f;

		public bool disableWhileHolding = true;

		[Header("Events")]
		public UnityHandEvent OnShow;

		public UnityHandEvent OnHide;

		private bool showing;

		private void Update()
		{
			if (!(hand == null) && !(head == null))
			{
				Vector3 position = hand.transform.position;
				Vector3 position2 = head.transform.position;
				float num = Vector3.Dot((position2 - position).normalized, -hand.palmTransform.forward);
				float num2 = Vector3.Distance(position2, hand.palmTransform.position);
				bool flag = num >= anglePreciseness && num2 < maxDistance && hand.holdingObj == null;
				if (!showing && flag)
				{
					OnShow?.Invoke(hand);
					showing = true;
				}
				else if (showing && !flag)
				{
					OnHide?.Invoke(hand);
					showing = false;
				}
			}
		}
	}
}
