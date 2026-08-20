using System.Collections.Generic;
using UnityEngine;

namespace Autohand
{
	[HelpURL("https://earnestrobot.notion.site/Custom-Poses-868c1fa0590542a0b5b7937b5feb6b0d")]
	public class GrabbablePose : MonoBehaviour
	{
		[AutoHeader("Grabbable Pose", 0, 0)]
		public bool ignoreMe;

		public bool poseEnabled = true;

		[Tooltip("Purely for organizational purposes in the editor")]
		public string poseName = "";

		[Tooltip("This value must match the pose index of the a hand in order for the pose to work")]
		public int poseIndex;

		[Tooltip("Whether or not this pose can be used by both hands at once or only one hand at a time")]
		public bool singleHanded;

		[AutoToggleHeader("Advanced Settings", 0, 0)]
		public bool showAdvanced = true;

		public float positionWeight = 1f;

		public float rotationWeight = 1f;

		[Tooltip("These poses will only be enabled when this pose is active. Great for secondary poses like holding the front of a gun with your second hand, only while holding the trigger")]
		public GrabbablePose[] linkedPoses;

		[HideInInspector]
		public bool showEditorTools = true;

		[Tooltip("Scriptable options NOT REQUIRED -> Create scriptable throught [Auto Hand/Custom Pose]")]
		[HideInInspector]
		public HandPoseScriptable poseScriptable;

		[Tooltip("Used to pose for the grabbable")]
		[HideInInspector]
		public Hand editorHand;

		[HideInInspector]
		public HandPoseData rightPose;

		[HideInInspector]
		public bool rightPoseSet;

		[HideInInspector]
		public HandPoseData leftPose;

		[HideInInspector]
		public bool leftPoseSet;

		private List<Hand> posingHands = new List<Hand>();

		protected virtual void Awake()
		{
			if (poseScriptable != null)
			{
				if (poseScriptable.leftSaved)
				{
					leftPoseSet = true;
				}
				if (poseScriptable.rightSaved)
				{
					rightPoseSet = true;
				}
			}
			for (int i = 0; i < linkedPoses.Length; i++)
			{
				linkedPoses[i].poseEnabled = false;
			}
		}

		public bool CanSetPose(Hand hand)
		{
			if (singleHanded && posingHands.Count > 0 && !posingHands.Contains(hand))
			{
				return false;
			}
			if (hand.poseIndex != poseIndex)
			{
				return false;
			}
			if (hand.left && !leftPoseSet)
			{
				return false;
			}
			if (!hand.left && !rightPoseSet)
			{
				return false;
			}
			return poseEnabled;
		}

		public virtual HandPoseData GetHandPoseData(Hand hand)
		{
			if (poseScriptable != null)
			{
				if (!hand.left)
				{
					return poseScriptable.rightPose;
				}
				return poseScriptable.leftPose;
			}
			if (!hand.left)
			{
				return rightPose;
			}
			return leftPose;
		}

		public virtual void SetHandPose(Hand hand, bool isProjection = false)
		{
			if (!isProjection)
			{
				if (!posingHands.Contains(hand))
				{
					posingHands.Add(hand);
				}
				for (int i = 0; i < linkedPoses.Length; i++)
				{
					linkedPoses[i].poseEnabled = true;
				}
			}
			GetHandPoseData(hand).SetPose(hand, base.transform);
		}

		public virtual void CancelHandPose(Hand hand)
		{
			if (posingHands.Contains(hand))
			{
				posingHands.Remove(hand);
			}
			for (int i = 0; i < linkedPoses.Length; i++)
			{
				linkedPoses[i].poseEnabled = false;
			}
		}

		public HandPoseData GetNewPoseData(Hand hand)
		{
			HandPoseData result = default(HandPoseData);
			List<Vector3> posePositionsList = new List<Vector3>();
			List<Quaternion> poseRotationsList = new List<Quaternion>();
			Transform transformRuler = AutoHandExtensions.transformRuler;
			transformRuler.position = base.transform.position;
			transformRuler.rotation = base.transform.rotation;
			transformRuler.localScale = base.transform.lossyScale;
			Transform transformRulerChild = AutoHandExtensions.transformRulerChild;
			transformRulerChild.position = hand.transform.position;
			transformRulerChild.rotation = hand.transform.rotation;
			result.handOffset = transformRulerChild.localPosition;
			result.localQuaternionOffset = transformRulerChild.localRotation;
			transformRuler.localScale = Vector3.one;
			Finger[] fingers = hand.fingers;
			for (int i = 0; i < fingers.Length; i++)
			{
				AssignChildrenPose(fingers[i].transform);
			}
			result.posePositions = new Vector3[posePositionsList.Count];
			result.poseRotations = new Quaternion[posePositionsList.Count];
			for (int j = 0; j < posePositionsList.Count; j++)
			{
				result.posePositions[j] = posePositionsList[j];
				result.poseRotations[j] = poseRotationsList[j];
			}
			return result;
			void AddPoint(Vector3 pos, Quaternion rot)
			{
				posePositionsList.Add(pos);
				poseRotationsList.Add(rot);
			}
			void AssignChildrenPose(Transform obj)
			{
				AddPoint(obj.localPosition, obj.localRotation);
				for (int k = 0; k < obj.childCount; k++)
				{
					AssignChildrenPose(obj.GetChild(k));
				}
			}
		}

		public bool HasPose(bool left)
		{
			if (poseScriptable != null && (left ? poseScriptable.leftSaved : poseScriptable.rightSaved))
			{
				if (!left)
				{
					return poseScriptable.rightSaved;
				}
				return poseScriptable.leftSaved;
			}
			if (!left)
			{
				return rightPoseSet;
			}
			return leftPoseSet;
		}
	}
}
