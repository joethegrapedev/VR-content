using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Autohand
{
	public class Stabber : MonoBehaviour
	{
		[Tooltip("Can be left empty/null")]
		public Grabbable grabbable;

		[Header("Stab Settings")]
		public CapsuleCollider stabCapsule;

		[Tooltip("If left empty, will default to grabbable layers")]
		public LayerMask stabbableLayers;

		[Tooltip("The index that must match the stabbables index to allow stabbing")]
		public int stabIndex;

		public int maxStabs = 3;

		[Header("Joint Settings")]
		public Vector3 axis;

		public float limit = float.MaxValue;

		public ConfigurableJointMotion xMotion;

		public ConfigurableJointMotion yMotion;

		public ConfigurableJointMotion zMotion;

		public ConfigurableJointMotion angularXMotion;

		public ConfigurableJointMotion angularYMotion;

		public ConfigurableJointMotion angularZMotion;

		[Space]
		public float positionDampeningMultiplyer = 1f;

		public float rotationDampeningMultiplyer = 1f;

		[Header("Events")]
		public UnityEvent StartStab;

		public UnityEvent EndStab;

		public StabEvent StartStabEvent;

		public StabEvent EndStabEvent;

		private List<Stabbable> stabbed;

		private List<ConfigurableJoint> stabbedJoints;

		private Dictionary<Stabbable, int> stabbedFrames;

		private Collider[] resultsNonAlloc;

		private const int STABFRAMES = 3;

		private Vector3 startPos;

		private Quaternion startRot;

		private Vector3 lastPos;

		private Quaternion lastRot;

		private int frames;

		private Dictionary<Transform, Transform> originalParents = new Dictionary<Transform, Transform>();

		private void Start()
		{
			stabbedFrames = new Dictionary<Stabbable, int>();
			stabbed = new List<Stabbable>();
			stabbedJoints = new List<ConfigurableJoint>();
			resultsNonAlloc = new Collider[25];
			if ((int)stabbableLayers == 0)
			{
				stabbableLayers = LayerMask.GetMask(Hand.grabbableLayers);
			}
			StartStabEvent = (StabEvent)Delegate.Combine(StartStabEvent, (StabEvent)delegate
			{
				StartStab?.Invoke();
			});
			EndStabEvent = (StabEvent)Delegate.Combine(EndStabEvent, (StabEvent)delegate
			{
				EndStab?.Invoke();
			});
			startPos = base.transform.position;
			startRot = base.transform.rotation;
			base.gameObject.CanGetComponent<Grabbable>(out grabbable);
			StartCoroutine(StartWait());
		}

		private IEnumerator StartWait()
		{
			for (int i = 0; i < 3; i++)
			{
				base.transform.position = startPos;
				base.transform.rotation = startRot;
				yield return new WaitForFixedUpdate();
			}
		}

		private void FixedUpdate()
		{
			if (base.transform.position != lastPos || lastRot != base.transform.rotation)
			{
				frames = 0;
				lastPos = base.transform.position;
				lastRot = base.transform.rotation;
			}
			if (frames < 3)
			{
				CheckStabArea();
				frames++;
			}
		}

		protected virtual void CheckStabArea()
		{
			float height = stabCapsule.height;
			float radius = stabCapsule.radius;
			Vector3 vector;
			if (stabCapsule.direction == 0)
			{
				vector = Vector3.right;
				height *= stabCapsule.transform.lossyScale.x;
				radius *= ((stabCapsule.transform.lossyScale.y > stabCapsule.transform.lossyScale.z) ? stabCapsule.transform.lossyScale.y : stabCapsule.transform.lossyScale.z);
			}
			else if (stabCapsule.direction == 1)
			{
				vector = Vector3.up;
				height *= stabCapsule.transform.lossyScale.y;
				radius *= ((stabCapsule.transform.lossyScale.z > stabCapsule.transform.lossyScale.x) ? stabCapsule.transform.lossyScale.z : stabCapsule.transform.lossyScale.x);
			}
			else
			{
				vector = Vector3.forward;
				height *= stabCapsule.transform.lossyScale.z;
				radius *= ((stabCapsule.transform.lossyScale.y > stabCapsule.transform.lossyScale.x) ? stabCapsule.transform.lossyScale.y : stabCapsule.transform.lossyScale.x);
			}
			if (height / 2f <= radius)
			{
				height = 0f;
			}
			else
			{
				height /= 2f;
				height -= radius;
			}
			Vector3 point = stabCapsule.bounds.center + stabCapsule.transform.rotation * vector * height;
			Vector3 point2 = stabCapsule.bounds.center - stabCapsule.transform.rotation * vector * height;
			Physics.OverlapCapsuleNonAlloc(point, point2, radius, resultsNonAlloc, stabbableLayers, QueryTriggerInteraction.Ignore);
			List<Stabbable> list = new List<Stabbable>();
			for (int i = 0; i < resultsNonAlloc.Length; i++)
			{
				if (resultsNonAlloc[i] != null && resultsNonAlloc[i].CanGetComponent<Stabbable>(out var component) && component.gameObject != base.gameObject)
				{
					list.Add(component);
				}
			}
			for (int num = stabbed.Count - 1; num >= 0; num--)
			{
				if (!list.Contains(stabbed[num]))
				{
					OnStabbableExit(stabbed[num]);
				}
			}
			if (stabbed.Count < maxStabs)
			{
				for (int j = 0; j < list.Count; j++)
				{
					if (!stabbed.Contains(list[j]) && list[j].CanStab(this))
					{
						OnStabbableEnter(list[j]);
					}
				}
			}
			for (int k = 0; k < resultsNonAlloc.Length; k++)
			{
				resultsNonAlloc[k] = null;
			}
			if (stabbedFrames.Count > 0)
			{
				Stabbable[] array = new Stabbable[stabbedFrames.Count];
				stabbedFrames.Keys.CopyTo(array, 0);
				Stabbable[] array2 = array;
				foreach (Stabbable stabbable in array2)
				{
					if (!stabbed.Contains(stabbable) && !list.Contains(stabbable))
					{
						stabbedFrames.Remove(stabbable);
					}
				}
			}
			list.Clear();
		}

		protected virtual void OnStabbableEnter(Stabbable stab)
		{
			if (stabbedFrames.ContainsKey(stab))
			{
				stabbedFrames[stab]++;
			}
			else
			{
				stabbedFrames.Add(stab, 1);
			}
			if (stabbedFrames[stab] >= 3)
			{
				stabbed.Add(stab);
				ConfigurableJoint configurableJoint = base.gameObject.AddComponent<ConfigurableJoint>();
				configurableJoint.secondaryAxis = axis;
				configurableJoint.connectedBody = stab.body;
				configurableJoint.xMotion = xMotion;
				configurableJoint.yMotion = yMotion;
				configurableJoint.zMotion = zMotion;
				configurableJoint.angularXMotion = angularXMotion;
				configurableJoint.angularYMotion = angularYMotion;
				configurableJoint.angularZMotion = angularZMotion;
				configurableJoint.linearLimit = new SoftJointLimit
				{
					limit = limit
				};
				configurableJoint.linearLimitSpring = new SoftJointLimitSpring
				{
					damper = stab.positionDamper * positionDampeningMultiplyer
				};
				configurableJoint.xDrive = new JointDrive
				{
					positionDamper = stab.positionDamper * positionDampeningMultiplyer,
					maximumForce = float.MaxValue
				};
				configurableJoint.yDrive = new JointDrive
				{
					positionDamper = stab.positionDamper * positionDampeningMultiplyer,
					maximumForce = float.MaxValue
				};
				configurableJoint.zDrive = new JointDrive
				{
					positionDamper = stab.positionDamper * positionDampeningMultiplyer,
					maximumForce = float.MaxValue
				};
				configurableJoint.slerpDrive = new JointDrive
				{
					positionDamper = stab.positionDamper * positionDampeningMultiplyer
				};
				configurableJoint.angularXLimitSpring = new SoftJointLimitSpring
				{
					damper = stab.rotationDamper * rotationDampeningMultiplyer
				};
				configurableJoint.angularYZLimitSpring = new SoftJointLimitSpring
				{
					damper = stab.rotationDamper * rotationDampeningMultiplyer
				};
				configurableJoint.angularXDrive = new JointDrive
				{
					positionDamper = stab.rotationDamper * rotationDampeningMultiplyer,
					maximumForce = float.MaxValue
				};
				configurableJoint.angularYZDrive = new JointDrive
				{
					positionDamper = stab.rotationDamper * rotationDampeningMultiplyer,
					maximumForce = float.MaxValue
				};
				configurableJoint.projectionDistance /= 4f;
				configurableJoint.enablePreprocessing = true;
				configurableJoint.enableCollision = false;
				configurableJoint.CanGetComponent<Rigidbody>(out var component);
				component.detectCollisions = false;
				component.detectCollisions = true;
				stab.body.WakeUp();
				component.WakeUp();
				stabbedJoints.Add(configurableJoint);
				stab.OnStab(this);
				StartStabEvent?.Invoke(this, stab);
				if (stab.parentOnStab && (bool)grabbable)
				{
					grabbable.AddJointedBody(stab.body);
				}
				else if ((bool)grabbable)
				{
					grabbable.ignoreParent = true;
				}
			}
		}

		protected virtual void OnStabbableExit(Stabbable stab)
		{
			int index = stabbed.IndexOf(stab);
			stabbed.Remove(stab);
			ConfigurableJoint obj = stabbedJoints[index];
			stabbedJoints.RemoveAt(index);
			UnityEngine.Object.Destroy(obj);
			stab.OnEndStab(this);
			stabbedFrames.Remove(stab);
			EndStabEvent?.Invoke(this, stab);
			if (stab.parentOnStab && (bool)grabbable)
			{
				grabbable.RemoveJointedBody(stab.body);
			}
			else if ((bool)grabbable)
			{
				grabbable.ignoreParent = false;
			}
		}

		public List<Stabbable> GetStabbed()
		{
			return stabbed;
		}

		public int GetStabbedCount()
		{
			return stabbed.Count;
		}

		private void OnDrawGizmosSelected()
		{
			float height = stabCapsule.height;
			float radius = stabCapsule.radius;
			Vector3 vector;
			if (stabCapsule.direction == 0)
			{
				vector = Vector3.right;
				height *= stabCapsule.transform.lossyScale.x;
				radius *= ((stabCapsule.transform.lossyScale.y > stabCapsule.transform.lossyScale.z) ? stabCapsule.transform.lossyScale.y : stabCapsule.transform.lossyScale.z);
			}
			else if (stabCapsule.direction == 1)
			{
				vector = Vector3.up;
				height *= stabCapsule.transform.lossyScale.y;
				radius *= ((stabCapsule.transform.lossyScale.z > stabCapsule.transform.lossyScale.x) ? stabCapsule.transform.lossyScale.z : stabCapsule.transform.lossyScale.x);
			}
			else
			{
				vector = Vector3.forward;
				height *= stabCapsule.transform.lossyScale.z;
				radius *= ((stabCapsule.transform.lossyScale.y > stabCapsule.transform.lossyScale.x) ? stabCapsule.transform.lossyScale.y : stabCapsule.transform.lossyScale.x);
			}
			if (height / 2f <= radius)
			{
				height = 0f;
			}
			else
			{
				height /= 2f;
				height -= radius;
			}
			Vector3 center = stabCapsule.bounds.center + stabCapsule.transform.rotation * vector * height;
			Vector3 center2 = stabCapsule.bounds.center - stabCapsule.transform.rotation * vector * height;
			Gizmos.color = Color.blue;
			Gizmos.DrawSphere(center, radius);
			Gizmos.DrawSphere(center2, radius);
		}
	}
}
