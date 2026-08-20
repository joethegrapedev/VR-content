using UnityEngine;

namespace Autohand.Demo
{
	public class DialHandScaler : PhysicsGadgetHingeAngleReader
	{
		public Hand hand;

		public Vector3 minScale;

		public Vector3 maxScale;

		private float startReach;

		private Vector3 startScale;

		private float[] fingersStartScale;

		private Vector3 lastHandScale;

		protected new void Start()
		{
			base.Start();
			startScale = hand.transform.localScale;
			startReach = hand.reachDistance;
			fingersStartScale = new float[hand.fingers.Length];
			for (int i = 0; i < hand.fingers.Length; i++)
			{
				fingersStartScale[i] = hand.fingers[i].tipRadius;
			}
			lastHandScale = hand.transform.localScale;
		}

		private void Update()
		{
			float num = GetValue();
			float num2 = hand.transform.localScale.magnitude / startScale.magnitude;
			if (num >= 0f)
			{
				hand.transform.localScale = Vector3.Lerp(startScale, maxScale, num);
			}
			else if (num < 0f)
			{
				hand.transform.localScale = Vector3.Lerp(startScale, minScale, 0f - num);
			}
			hand.reachDistance = startReach * num2;
			for (int i = 0; i < hand.fingers.Length; i++)
			{
				hand.fingers[i].tipRadius = fingersStartScale[i] * num2;
			}
			if (hand.transform.localScale != lastHandScale)
			{
				hand.ForceReleaseGrab();
			}
			lastHandScale = hand.transform.localScale;
		}
	}
}
