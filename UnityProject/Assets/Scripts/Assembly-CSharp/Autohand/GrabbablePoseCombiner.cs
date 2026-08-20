using System.Collections.Generic;
using UnityEngine;

namespace Autohand
{
	public class GrabbablePoseCombiner : MonoBehaviour
	{
		public List<GrabbablePose> poses = new List<GrabbablePose>();

		private HandPoseData pose;

		public bool CanSetPose(Hand hand)
		{
			foreach (GrabbablePose pose in poses)
			{
				if (pose != null && pose.CanSetPose(hand))
				{
					return true;
				}
			}
			return false;
		}

		public void AddPose(GrabbablePose pose)
		{
			if (!poses.Contains(pose))
			{
				poses.Add(pose);
			}
		}

		private void OnDestroy()
		{
			for (int num = poses.Count - 1; num >= 0; num--)
			{
				Object.Destroy(poses[num]);
			}
		}

		public GrabbablePose GetClosestPose(Hand hand)
		{
			List<GrabbablePose> list = new List<GrabbablePose>();
			foreach (GrabbablePose pose in poses)
			{
				if (pose != null && pose.CanSetPose(hand))
				{
					list.Add(pose);
				}
			}
			float num = float.MaxValue;
			int index = 0;
			for (int i = 0; i < list.Count; i++)
			{
				Vector3 position = hand.transform.position;
				Quaternion rotation = hand.transform.rotation;
				Transform transformRuler = AutoHandExtensions.transformRuler;
				transformRuler.rotation = Quaternion.identity;
				transformRuler.position = list[i].transform.position;
				transformRuler.localScale = list[i].transform.lossyScale;
				Transform transformRulerChild = AutoHandExtensions.transformRulerChild;
				transformRulerChild.position = hand.transform.position;
				transformRulerChild.rotation = hand.transform.rotation;
				this.pose = list[i].GetHandPoseData(hand);
				transformRulerChild.localPosition = this.pose.handOffset;
				transformRulerChild.localRotation = this.pose.localQuaternionOffset;
				float num2 = Vector3.Distance(transformRulerChild.position, position);
				float num3 = Quaternion.Angle(transformRulerChild.rotation, rotation) / 90f;
				float num4 = num2 / list[i].positionWeight + num3 / list[i].rotationWeight;
				if (num4 < num)
				{
					index = i;
					num = num4;
				}
				hand.transform.position = position;
				hand.transform.rotation = rotation;
			}
			return list[index];
		}

		internal int PoseCount()
		{
			return poses.Count;
		}
	}
}
