using UnityEngine;

namespace Autohand
{
	[HelpURL("https://earnestrobot.notion.site/Fingers-63ae83cda0b14a35b5ae15beeb51dc03")]
	public class Finger : MonoBehaviour
	{
		[Header("Tips")]
		[Tooltip("This transfrom will represent the tip/stopper of the finger")]
		public Transform tip;

		[Tooltip("This determines the radius of the spherecast check when bending fingers")]
		public float tipRadius = 0.01f;

		[Tooltip("This will offset the fingers bend (0 is no bend, 1 is full bend)")]
		[Range(0f, 1f)]
		public float bendOffset;

		public float fingerSmoothSpeed = 1f;

		[HideInInspector]
		public float secondaryOffset;

		private float currBendOffset;

		private float bend;

		[SerializeField]
		[HideInInspector]
		private Quaternion[] minGripRotPose;

		[SerializeField]
		[HideInInspector]
		private Vector3[] minGripPosPose;

		[SerializeField]
		[HideInInspector]
		private Quaternion[] maxGripRotPose;

		[SerializeField]
		[HideInInspector]
		private Vector3[] maxGripPosPose;

		[SerializeField]
		[HideInInspector]
		private Transform[] fingerJoints;

		private float lastHitBend;

		private Collider[] results = new Collider[2];

		private void Update()
		{
			SlowBend();
		}

		public bool BendFingerUntilHit(int steps, int layermask)
		{
			ResetBend();
			lastHitBend = 0f;
			for (float num = 0f; num <= (float)steps / 5f; num += 1f)
			{
				results[0] = null;
				lastHitBend = num / ((float)steps / 5f);
				for (int i = 0; i < fingerJoints.Length; i++)
				{
					fingerJoints[i].localPosition = Vector3.Lerp(minGripPosPose[i], maxGripPosPose[i], lastHitBend);
					fingerJoints[i].localRotation = Quaternion.Lerp(minGripRotPose[i], maxGripRotPose[i], lastHitBend);
				}
				Physics.OverlapSphereNonAlloc(tip.transform.position, tipRadius, results, layermask, QueryTriggerInteraction.Ignore);
				if (results[0] != null)
				{
					lastHitBend = Mathf.Clamp01(lastHitBend);
					if (num != 0f)
					{
						break;
					}
					return true;
				}
			}
			lastHitBend -= 5f / (float)steps;
			for (int j = 0; (float)j <= (float)steps / 10f; j++)
			{
				results[0] = null;
				lastHitBend += 1f / (float)steps;
				for (int k = 0; k < fingerJoints.Length; k++)
				{
					fingerJoints[k].localPosition = Vector3.Lerp(minGripPosPose[k], maxGripPosPose[k], lastHitBend);
					fingerJoints[k].localRotation = Quaternion.Lerp(minGripRotPose[k], maxGripRotPose[k], lastHitBend);
				}
				Physics.OverlapSphereNonAlloc(tip.transform.position, tipRadius, results, layermask, QueryTriggerInteraction.Ignore);
				if (results[0] != null)
				{
					bend = lastHitBend;
					currBendOffset = lastHitBend;
					lastHitBend = Mathf.Clamp01(lastHitBend);
					return true;
				}
				if (lastHitBend >= 1f)
				{
					lastHitBend = Mathf.Clamp01(lastHitBend);
					return true;
				}
			}
			return false;
		}

		public bool UpdateFingerBend(float bend, int layermask)
		{
			Collider[] array = new Collider[1];
			Physics.OverlapSphereNonAlloc(tip.transform.position, tipRadius, array, layermask, QueryTriggerInteraction.Ignore);
			if (this.bend > bend || array[0] == null)
			{
				this.bend = bend;
				for (int i = 0; i < fingerJoints.Length; i++)
				{
					fingerJoints[i].localPosition = Vector3.Lerp(minGripPosPose[i], maxGripPosPose[i], currBendOffset + secondaryOffset);
					fingerJoints[i].localRotation = Quaternion.Lerp(minGripRotPose[i], maxGripRotPose[i], currBendOffset + secondaryOffset);
				}
				return true;
			}
			return false;
		}

		public void UpdateFinger()
		{
			for (int i = 0; i < fingerJoints.Length; i++)
			{
				fingerJoints[i].localPosition = Vector3.Lerp(minGripPosPose[i], maxGripPosPose[i], currBendOffset + secondaryOffset);
				fingerJoints[i].localRotation = Quaternion.Lerp(minGripRotPose[i], maxGripRotPose[i], currBendOffset + secondaryOffset);
			}
		}

		public void UpdateFinger(float bend)
		{
			this.bend = bend;
			for (int i = 0; i < fingerJoints.Length; i++)
			{
				fingerJoints[i].localPosition = Vector3.Lerp(minGripPosPose[i], maxGripPosPose[i], currBendOffset + secondaryOffset);
				fingerJoints[i].localRotation = Quaternion.Lerp(minGripRotPose[i], maxGripRotPose[i], currBendOffset + secondaryOffset);
			}
		}

		public void SetFingerBend(float bend)
		{
			this.bend = bend;
			for (int i = 0; i < fingerJoints.Length; i++)
			{
				fingerJoints[i].localPosition = Vector3.Lerp(minGripPosPose[i], maxGripPosPose[i], bend);
				fingerJoints[i].localRotation = Quaternion.Lerp(minGripRotPose[i], maxGripRotPose[i], bend);
			}
		}

		public void SetCurrentFingerBend(float bend)
		{
			currBendOffset = bend;
			for (int i = 0; i < fingerJoints.Length; i++)
			{
				fingerJoints[i].localPosition = Vector3.Lerp(minGripPosPose[i], maxGripPosPose[i], bend);
				fingerJoints[i].localRotation = Quaternion.Lerp(minGripRotPose[i], maxGripRotPose[i], bend);
			}
		}

		private void SlowBend()
		{
			float num = bendOffset + bend;
			if (currBendOffset != num)
			{
				currBendOffset = Mathf.MoveTowards(currBendOffset, num, 6f * fingerSmoothSpeed * Time.deltaTime);
			}
		}

		[ContextMenu("ResetBend")]
		public void ResetBend()
		{
			for (int i = 0; i < fingerJoints.Length; i++)
			{
				fingerJoints[i].localPosition = minGripPosPose[i];
				fingerJoints[i].localRotation = minGripRotPose[i];
			}
		}

		[ContextMenu("Grip")]
		public void Grip()
		{
			for (int i = 0; i < fingerJoints.Length; i++)
			{
				fingerJoints[i].localPosition = maxGripPosPose[i];
				fingerJoints[i].localRotation = maxGripRotPose[i];
			}
		}

		public float GetLastHitBend()
		{
			return lastHitBend;
		}

		[ContextMenu("Set Open Finger Pose")]
		public void SetMinPose()
		{
			int count = 0;
			GetKidsCount(base.transform, ref count);
			minGripPosPose = new Vector3[count];
			minGripRotPose = new Quaternion[count];
			fingerJoints = new Transform[count];
			int index = 0;
			AssignChildrenPose(base.transform, ref index);
			void AssignChildrenPose(Transform obj, ref int reference)
			{
				if (obj != tip)
				{
					AssignPoint(reference, obj.localPosition, obj.localRotation, obj);
					reference++;
					for (int i = 0; i < obj.childCount; i++)
					{
						AssignChildrenPose(obj.GetChild(i), ref reference);
					}
				}
			}
			void AssignPoint(int point, Vector3 pos, Quaternion rot, Transform joint)
			{
				minGripPosPose[point] = pos;
				minGripRotPose[point] = rot;
				fingerJoints[point] = joint;
			}
			int GetKidsCount(Transform obj, ref int reference)
			{
				if (obj != tip)
				{
					reference++;
					for (int i = 0; i < obj.childCount; i++)
					{
						GetKidsCount(obj.GetChild(i), ref reference);
					}
				}
				return reference;
			}
		}

		[ContextMenu("Set Closed Finger Pose")]
		public void SetMaxPose()
		{
			int count = 0;
			GetKidsCount(base.transform, ref count);
			maxGripPosPose = new Vector3[count];
			maxGripRotPose = new Quaternion[count];
			fingerJoints = new Transform[count];
			int index = 0;
			AssignChildrenPose(base.transform, ref index);
			void AssignChildrenPose(Transform obj, ref int reference)
			{
				if (obj != tip)
				{
					AssignPoint(reference, obj.localPosition, obj.localRotation, obj);
					reference++;
					for (int i = 0; i < obj.childCount; i++)
					{
						AssignChildrenPose(obj.GetChild(i), ref reference);
					}
				}
			}
			void AssignPoint(int point, Vector3 pos, Quaternion rot, Transform joint)
			{
				maxGripPosPose[point] = pos;
				maxGripRotPose[point] = rot;
				fingerJoints[point] = joint;
			}
			int GetKidsCount(Transform obj, ref int reference)
			{
				if (obj != tip)
				{
					reference++;
					for (int i = 0; i < obj.childCount; i++)
					{
						GetKidsCount(obj.GetChild(i), ref reference);
					}
				}
				return reference;
			}
		}

		public void CopyPose(Finger finger)
		{
			maxGripPosPose = new Vector3[finger.maxGripPosPose.Length];
			finger.maxGripPosPose.CopyTo(maxGripPosPose, 0);
			maxGripRotPose = new Quaternion[finger.maxGripRotPose.Length];
			finger.maxGripRotPose.CopyTo(maxGripRotPose, 0);
			minGripPosPose = new Vector3[finger.minGripPosPose.Length];
			finger.minGripPosPose.CopyTo(minGripPosPose, 0);
			minGripRotPose = new Quaternion[finger.minGripRotPose.Length];
			finger.minGripRotPose.CopyTo(minGripRotPose, 0);
			fingerJoints = new Transform[finger.fingerJoints.Length];
			finger.fingerJoints.CopyTo(fingerJoints, 0);
		}

		public bool IsMinPoseSaved()
		{
			return minGripPosPose.Length != 0;
		}

		public bool IsMaxPoseSaved()
		{
			return maxGripPosPose.Length != 0;
		}

		public float GetCurrentBend()
		{
			return currBendOffset + secondaryOffset;
		}

		private void OnDrawGizmos()
		{
			if (!(tip == null))
			{
				Gizmos.color = Color.cyan;
				Gizmos.DrawWireSphere(tip.transform.position, tipRadius);
			}
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.blue;
			DrawSphereBetweenChild(base.transform);
			void DrawSphereBetweenChild(Transform transform)
			{
				for (int i = 0; i < transform.childCount; i++)
				{
					Transform child = transform.GetChild(i);
					if (child.TryGetComponent<CapsuleCollider>(out var component))
					{
						Gizmos.DrawWireSphere(Vector3.Lerp(transform.position, component.bounds.center, 0.5f), tipRadius);
					}
					DrawSphereBetweenChild(child);
				}
			}
		}
	}
}
