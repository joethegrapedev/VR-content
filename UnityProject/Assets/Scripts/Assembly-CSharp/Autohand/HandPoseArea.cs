using System.Collections.Generic;
using UnityEngine;

namespace Autohand
{
	[HelpURL("https://www.notion.so/Pose-Areas-99b9af26d297442a91a9d73f65f13635")]
	public class HandPoseArea : MonoBehaviour
	{
		public string poseName;

		public int poseIndex;

		public float transitionTime = 0.2f;

		[Header("Events")]
		public UnityHandEvent OnHandEnter = new UnityHandEvent();

		public UnityHandEvent OnHandExit = new UnityHandEvent();

		[HideInInspector]
		[Tooltip("Scriptable options NOT REQUIRED (will be saved locally instead if empty) -> Create scriptable throught [Auto Hand/Custom Pose]")]
		public HandPoseScriptable poseScriptable;

		[HideInInspector]
		public HandPoseData rightPose;

		[HideInInspector]
		public bool rightPoseSet;

		[HideInInspector]
		public HandPoseData leftPose;

		[HideInInspector]
		public bool leftPoseSet;

		internal HandPoseArea[] poseAreas;

		private List<Hand> posingHands = new List<Hand>();

		private void Start()
		{
			poseAreas = GetComponents<HandPoseArea>();
		}

		private void OnEnable()
		{
			OnHandEnter.AddListener(HandEnter);
			OnHandExit.AddListener(HandExit);
		}

		private void OnDisable()
		{
			for (int i = 0; i < posingHands.Count; i++)
			{
				posingHands[i].TryRemoveHandPoseArea(this);
			}
			OnHandEnter.RemoveListener(HandEnter);
			OnHandExit.RemoveListener(HandExit);
		}

		private void HandEnter(Hand hand)
		{
			posingHands.Add(hand);
		}

		private void HandExit(Hand hand)
		{
			posingHands.Remove(hand);
		}

		public virtual HandPoseData GetHandPoseData(bool left)
		{
			if (poseScriptable != null)
			{
				if (!left)
				{
					return poseScriptable.rightPose;
				}
				return poseScriptable.leftPose;
			}
			if (!left)
			{
				return rightPose;
			}
			return leftPose;
		}

		public void SetHandPose(Hand hand)
		{
			HandPoseData handPoseData;
			if (hand.left)
			{
				if (!leftPoseSet)
				{
					return;
				}
				handPoseData = leftPose;
			}
			else
			{
				if (!rightPoseSet)
				{
					return;
				}
				handPoseData = rightPose;
			}
			handPoseData.SetPose(hand, base.transform);
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
