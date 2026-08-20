using System;
using System.Collections.Generic;
using UnityEngine;

namespace Autohand
{
	[Serializable]
	public struct HandPoseData
	{
		public Vector3 handOffset;

		public Vector3 rotationOffset;

		public Quaternion localQuaternionOffset;

		public Vector3[] posePositions;

		public Quaternion[] poseRotations;

		public HandPoseData(Hand hand, Grabbable grabbable)
		{
			posePositions = new Vector3[0];
			poseRotations = new Quaternion[0];
			handOffset = default(Vector3);
			rotationOffset = Vector3.zero;
			localQuaternionOffset = Quaternion.identity;
			SavePose(hand, grabbable.transform);
		}

		public HandPoseData(Hand hand, Transform point)
		{
			posePositions = new Vector3[0];
			poseRotations = new Quaternion[0];
			handOffset = default(Vector3);
			rotationOffset = Vector3.zero;
			localQuaternionOffset = Quaternion.identity;
			SavePose(hand, point);
		}

		public HandPoseData(Hand hand)
		{
			posePositions = new Vector3[0];
			poseRotations = new Quaternion[0];
			handOffset = default(Vector3);
			rotationOffset = Vector3.zero;
			localQuaternionOffset = Quaternion.identity;
			SavePose(hand, null);
		}

		public HandPoseData(HandPoseData data)
		{
			posePositions = new Vector3[data.posePositions.Length];
			data.posePositions.CopyTo(posePositions, 0);
			poseRotations = new Quaternion[data.poseRotations.Length];
			data.poseRotations.CopyTo(poseRotations, 0);
			handOffset = data.handOffset;
			rotationOffset = data.rotationOffset;
			localQuaternionOffset = data.localQuaternionOffset;
		}

		public void SavePose(Hand hand, Transform relativeTo)
		{
			List<Vector3> posePositionsList = new List<Vector3>();
			List<Quaternion> poseRotationsList = new List<Quaternion>();
			if (relativeTo != null)
			{
				Transform transformRuler = AutoHandExtensions.transformRuler;
				transformRuler.localScale = relativeTo.lossyScale;
				transformRuler.transform.position = relativeTo.position;
				transformRuler.transform.rotation = relativeTo.rotation;
				Transform transformRulerChild = AutoHandExtensions.transformRulerChild;
				transformRulerChild.transform.position = hand.transform.position;
				transformRulerChild.transform.rotation = hand.transform.rotation;
				handOffset = transformRulerChild.localPosition;
				localQuaternionOffset = transformRulerChild.localRotation;
			}
			else
			{
				handOffset = hand.transform.localPosition;
				localQuaternionOffset = hand.transform.localRotation;
			}
			rotationOffset = Vector3.zero;
			Finger[] fingers = hand.fingers;
			for (int i = 0; i < fingers.Length; i++)
			{
				AssignChildrenPose(fingers[i].transform);
			}
			posePositions = new Vector3[posePositionsList.Count];
			poseRotations = new Quaternion[posePositionsList.Count];
			for (int j = 0; j < posePositionsList.Count; j++)
			{
				posePositions[j] = posePositionsList[j];
				poseRotations[j] = poseRotationsList[j];
			}
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

		public Quaternion GetRotationOffset()
		{
			if (rotationOffset != Vector3.zero)
			{
				localQuaternionOffset = Quaternion.Euler(rotationOffset);
			}
			return localQuaternionOffset;
		}

		public void SetPose(Hand hand, Transform relativeTo = null)
		{
			if (rotationOffset != Vector3.zero)
			{
				localQuaternionOffset = Quaternion.Euler(rotationOffset);
			}
			if (relativeTo != null && relativeTo != hand.transform)
			{
				Transform transformRuler = AutoHandExtensions.transformRuler;
				transformRuler.localScale = relativeTo.lossyScale;
				transformRuler.position = relativeTo.position;
				transformRuler.rotation = relativeTo.rotation;
				Transform transformRulerChild = AutoHandExtensions.transformRulerChild;
				transformRulerChild.localPosition = handOffset;
				transformRulerChild.localRotation = localQuaternionOffset;
				hand.transform.position = transformRulerChild.position;
				hand.transform.rotation = transformRulerChild.rotation;
			}
			int i = -1;
			if (posePositions != null)
			{
				Finger[] fingers = hand.fingers;
				for (int j = 0; j < fingers.Length; j++)
				{
					AssignChildrenPose(fingers[j].transform, this);
				}
			}
			void AssignChildrenPose(Transform obj, HandPoseData pose)
			{
				i++;
				obj.localPosition = pose.posePositions[i];
				obj.localRotation = pose.poseRotations[i];
				for (int k = 0; k < obj.childCount; k++)
				{
					AssignChildrenPose(obj.GetChild(k), pose);
				}
			}
		}

		public void SetFingerPose(Hand hand, Transform relativeTo = null)
		{
			int i = -1;
			if (posePositions != null)
			{
				Finger[] fingers = hand.fingers;
				for (int j = 0; j < fingers.Length; j++)
				{
					AssignChildrenPose(fingers[j].transform, this);
				}
			}
			void AssignChildrenPose(Transform obj, HandPoseData pose)
			{
				i++;
				obj.localPosition = pose.posePositions[i];
				obj.localRotation = pose.poseRotations[i];
				for (int k = 0; k < obj.childCount; k++)
				{
					AssignChildrenPose(obj.GetChild(k), pose);
				}
			}
		}

		public void SetPosition(Hand hand, Transform relativeTo = null)
		{
			if (rotationOffset != Vector3.zero)
			{
				localQuaternionOffset = Quaternion.Euler(rotationOffset);
			}
			if (relativeTo != null && relativeTo != hand.transform)
			{
				Transform transformRuler = AutoHandExtensions.transformRuler;
				transformRuler.localScale = relativeTo.lossyScale;
				transformRuler.position = relativeTo.position;
				transformRuler.rotation = relativeTo.rotation;
				Transform transformRulerChild = AutoHandExtensions.transformRulerChild;
				transformRulerChild.localPosition = handOffset;
				transformRulerChild.localRotation = localQuaternionOffset;
				hand.transform.position = transformRulerChild.position;
				hand.transform.rotation = transformRulerChild.rotation;
			}
		}

		public static HandPoseData LerpPose(HandPoseData from, HandPoseData to, float point)
		{
			HandPoseData result = new HandPoseData
			{
				handOffset = Vector3.Lerp(from.handOffset, to.handOffset, point),
				localQuaternionOffset = Quaternion.Lerp(from.localQuaternionOffset, to.localQuaternionOffset, point),
				posePositions = new Vector3[from.posePositions.Length],
				poseRotations = new Quaternion[from.poseRotations.Length]
			};
			for (int i = 0; i < from.posePositions.Length; i++)
			{
				result.posePositions[i] = Vector3.Lerp(from.posePositions[i], to.posePositions[i], point);
				result.poseRotations[i] = Quaternion.Lerp(from.poseRotations[i], to.poseRotations[i], point);
			}
			return result;
		}

		public static void LerpPose(ref HandPoseData lerpPose, HandPoseData from, HandPoseData to, float point)
		{
			lerpPose.handOffset = Vector3.Lerp(from.handOffset, to.handOffset, point);
			lerpPose.localQuaternionOffset = Quaternion.Lerp(from.localQuaternionOffset, to.localQuaternionOffset, point);
			lerpPose.posePositions = new Vector3[from.posePositions.Length];
			lerpPose.poseRotations = new Quaternion[from.poseRotations.Length];
			for (int i = 0; i < from.posePositions.Length; i++)
			{
				lerpPose.posePositions[i] = Vector3.Lerp(from.posePositions[i], to.posePositions[i], point);
				lerpPose.poseRotations[i] = Quaternion.Lerp(from.poseRotations[i], to.poseRotations[i], point);
			}
		}
	}
}
