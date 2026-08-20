using UnityEngine;

namespace Autohand
{
	[HelpURL("https://earnestrobot.notion.site/Custom-Poses-868c1fa0590542a0b5b7937b5feb6b0d")]
	public class GrabbablePoseAdvanced : GrabbablePose
	{
		[Tooltip("Usually this can be left empty, used to create a different center point if the objects transform isn't ceneterd for the prefered rotation/movement axis")]
		public Transform centerObject;

		[Space]
		[Tooltip("You want this set so the disc gizmo is around the axis you want the hand to rotate, or that the line is straight through the axis you want to move")]
		public Vector3 up = Vector3.up;

		[Space]
		[Tooltip("Whether or not to automatically allow for the opposite direction pose to be automatically applied (I.E. Should I be able to grab my hammer only with the head facing up, or in both directions?)")]
		public bool useInvertPose;

		[Space]
		[Tooltip("The minimum angle rotation around the included directions")]
		public int minAngle;

		[Tooltip("The maximum angle rotation around the included directions")]
		public int maxAngle = 360;

		[Space]
		[Tooltip("The minimum distance allowed from the saved posed along the included directions")]
		public float maxRange;

		[Tooltip("The maximum distance allowed from the saved posed along the included directions")]
		public float minRange;

		[Header("Requires Gizmos Enabled")]
		[Tooltip("Helps test pose by setting the angle of the editor hand, REQUIRES GIZMOS ENABLED")]
		public int testAngle;

		[Tooltip("Helps test pose by setting the range position of the editor hand, REQUIRES GIZMOS ENABLED")]
		public float testRange;

		private int lastAngle;

		private float lastRange;

		private Vector3 pregrabPos;

		private Quaternion pregrabRot;

		private Transform tempContainer;

		private Transform handMatch;

		private Transform getTransform;

		protected override void Awake()
		{
			base.Awake();
			if (minAngle > maxAngle)
			{
				int num = minAngle;
				minAngle = maxAngle;
				maxAngle = num;
			}
			if (minRange > maxRange)
			{
				float num2 = minRange;
				minRange = maxRange;
				maxRange = num2;
			}
		}

		public override HandPoseData GetHandPoseData(Hand hand)
		{
			pregrabPos = hand.transform.position;
			pregrabRot = hand.transform.rotation;
			HandPoseData newPoseData = GetNewPoseData(hand);
			base.GetHandPoseData(hand).SetPose(hand, base.transform);
			getTransform = GetTransform();
			tempContainer = AutoHandExtensions.transformRuler;
			tempContainer.rotation = Quaternion.identity;
			tempContainer.position = getTransform.position;
			tempContainer.localScale = getTransform.lossyScale;
			handMatch = AutoHandExtensions.transformRulerChild;
			handMatch.position = hand.transform.position;
			handMatch.rotation = hand.transform.rotation;
			tempContainer.rotation = getTransform.rotation;
			Quaternion closestRotation = GetClosestRotation(hand, up, useInvertPose);
			tempContainer.rotation = closestRotation;
			Vector3 closestPosition = GetClosestPosition(up);
			tempContainer.position = closestPosition;
			hand.transform.position = handMatch.position;
			hand.transform.rotation = handMatch.rotation;
			HandPoseData newPoseData2 = GetNewPoseData(hand);
			newPoseData.SetPose(hand);
			return newPoseData2;
		}

		public Quaternion GetClosestRotation(Hand hand, Vector3 up, bool addInverse)
		{
			tempContainer = AutoHandExtensions.transformRuler;
			tempContainer.rotation = Quaternion.identity;
			tempContainer.position = getTransform.position;
			tempContainer.localScale = getTransform.lossyScale;
			handMatch = AutoHandExtensions.transformRulerChild;
			handMatch.position = hand.transform.position;
			handMatch.rotation = hand.transform.rotation;
			tempContainer.rotation = getTransform.rotation;
			Quaternion rotation = tempContainer.rotation;
			float num = float.MaxValue;
			float num2 = 0f;
			float num3 = (float)(Mathf.Abs(minAngle) + Mathf.Abs(maxAngle)) / 10f;
			if (num3 == 0f)
			{
				num3 = 1f;
			}
			Vector3 vector = Vector3.zero;
			if (up.x != 0f)
			{
				vector = new Vector3(0f, 1f, 0f);
			}
			else if (up.y != 0f)
			{
				vector = new Vector3(1f, 0f, 0f);
			}
			else if (up.z != 0f)
			{
				vector = new Vector3(0f, 0f, 1f);
			}
			for (float num4 = minAngle; num4 <= (float)maxAngle; num4 += num3)
			{
				tempContainer.eulerAngles = getTransform.rotation * up;
				tempContainer.RotateAround(getTransform.position, getTransform.rotation * up, num4);
				float num5 = Vector3.Distance(handMatch.position, pregrabPos);
				num5 += Quaternion.Angle(handMatch.rotation, pregrabRot) / 180f;
				if (num5 < num)
				{
					num = num5;
					rotation = tempContainer.rotation;
					num2 = num4;
				}
			}
			for (float num6 = (0f - num3) / 2f; num6 < num3 / 2f; num6 += num3 / 10f)
			{
				tempContainer.eulerAngles = getTransform.rotation * up;
				tempContainer.RotateAround(getTransform.position, getTransform.rotation * up, num2 + num6);
				float num7 = Vector3.Distance(handMatch.position, pregrabPos);
				num7 += Quaternion.Angle(handMatch.rotation, pregrabRot) / 180f;
				if (num7 < num)
				{
					num = num7;
					rotation = tempContainer.rotation;
					num2 = num6;
				}
			}
			if (addInverse)
			{
				float num8 = float.MaxValue;
				float num9 = 0f;
				for (float num10 = minAngle; num10 <= (float)maxAngle; num10 += num3)
				{
					tempContainer.eulerAngles = getTransform.rotation * up;
					tempContainer.RotateAround(getTransform.position, getTransform.rotation * up, num10);
					tempContainer.RotateAround(getTransform.position, getTransform.rotation * vector, 180f);
					float num11 = Vector3.Distance(handMatch.position, pregrabPos);
					num11 += Quaternion.Angle(handMatch.rotation, pregrabRot) / 180f;
					if (num11 < num8)
					{
						num8 = num11;
						if (num8 < num)
						{
							rotation = tempContainer.rotation;
						}
						num9 = num10;
					}
				}
				for (float num12 = (0f - num3) / 2f; num12 < num3 / 2f; num12 += num3 / 10f)
				{
					tempContainer.eulerAngles = getTransform.rotation * up;
					tempContainer.RotateAround(getTransform.position, getTransform.rotation * up, num9 + num12);
					tempContainer.RotateAround(getTransform.position, getTransform.rotation * vector, 180f);
					float num13 = Vector3.Distance(handMatch.position, pregrabPos);
					num13 += Quaternion.Angle(handMatch.rotation, pregrabRot) / 180f;
					if (num13 < num8)
					{
						num8 = num13;
						if (num8 < num)
						{
							rotation = tempContainer.rotation;
						}
						num9 = num12;
					}
				}
			}
			return rotation;
		}

		public Vector3 GetClosestPosition(Vector3 up)
		{
			Vector3 position = tempContainer.position;
			if (minRange != 0f || maxRange != 0f)
			{
				float num = float.MaxValue;
				float num2 = 0f;
				Vector3 a = getTransform.position + getTransform.rotation * up * minRange;
				Vector3 b = getTransform.position + getTransform.rotation * up * maxRange;
				for (int i = 0; i < 10; i++)
				{
					tempContainer.position = Vector3.Lerp(a, b, (float)i / 10f);
					float num3 = Vector3.Distance(handMatch.position, pregrabPos);
					if (num3 < num)
					{
						num = num3;
						position = tempContainer.position;
						num2 = i;
					}
				}
				for (int j = -5; j < 5; j++)
				{
					tempContainer.position = Vector3.Lerp(a, b, num2 + (float)j / 100f);
					float num4 = Vector3.Distance(handMatch.position, pregrabPos);
					if (num4 < num)
					{
						num = num4;
						position = tempContainer.position;
					}
				}
			}
			return position;
		}

		public HandPoseData GetHandPoseData(Hand hand, int angle, float range)
		{
			base.GetHandPoseData(hand).SetPose(hand, base.transform);
			Transform transform = GetTransform();
			Transform transformRuler = AutoHandExtensions.transformRuler;
			transformRuler.rotation = Quaternion.identity;
			transformRuler.position = transform.position;
			transformRuler.localScale = transform.lossyScale;
			Transform transformRulerChild = AutoHandExtensions.transformRulerChild;
			transformRulerChild.position = hand.transform.position;
			transformRulerChild.rotation = hand.transform.rotation;
			transformRuler.rotation = transform.rotation;
			transformRuler.eulerAngles = transform.rotation * up;
			transformRuler.RotateAround(transformRuler.transform.position, transform.rotation * up, angle);
			transformRuler.transform.position = transform.position + transform.rotation * up * range;
			hand.transform.position = transformRulerChild.position;
			hand.transform.rotation = transformRulerChild.rotation;
			transformRuler.localScale = Vector3.one;
			return GetNewPoseData(hand);
		}

		private Transform GetTransform()
		{
			if (!(centerObject != null))
			{
				return base.transform;
			}
			return centerObject;
		}
	}
}
