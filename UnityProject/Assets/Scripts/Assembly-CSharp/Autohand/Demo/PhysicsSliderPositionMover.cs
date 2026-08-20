using UnityEngine;

namespace Autohand.Demo
{
	public class PhysicsSliderPositionMover : PhysicsGadgetConfigurableLimitReader
	{
		[Header("Movement")]
		public Transform move;

		[Tooltip("Acts as speed")]
		public Vector3 axis = Vector3.up;

		[Header("Range")]
		public bool useRange;

		public Vector3 minRange = -Vector3.up;

		public Vector3 maxRange = Vector3.up;

		private Vector3 startPos;

		protected new void Start()
		{
			base.Start();
			startPos = move.position;
		}

		public void FixedUpdate()
		{
			if (useRange)
			{
				float num = GetValue();
				if (num >= 0f)
				{
					move.position = Vector3.Lerp(startPos, startPos + minRange, num);
				}
				else if (num < 0f)
				{
					move.position = Vector3.Lerp(startPos, startPos + maxRange, 0f - num);
				}
			}
			else
			{
				move.position += axis * GetValue() * Time.fixedDeltaTime;
			}
		}
	}
}
