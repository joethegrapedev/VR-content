using System;
using NaughtyAttributes;
using UnityEngine;

namespace Autohand
{
	[RequireComponent(typeof(Grabbable))]
	[HelpURL("https://earnestrobot.notion.site/Distance-Grabbing-19e4e8b14f00428295eca75fca752787")]
	public class DistanceGrabbable : MonoBehaviour
	{
		[AutoHeader("Distance Grabbable", 0, 0)]
		public bool ignoreMe;

		[Header("Pull")]
		public bool instantPull = true;

		public DistanceGrabType grabType;

		[Range(0.4f, 1.1f)]
		[Tooltip("Use this to adjust the angle of the arch that the gameobject follows while shooting towards your hand.")]
		[ShowIf("grabType", DistanceGrabType.Velocity)]
		public float archMultiplier = 0.6f;

		[Tooltip("Slow down or speed up gravitation to your liking.")]
		[ShowIf("grabType", DistanceGrabType.Velocity)]
		public float gravitationVelocity = 1f;

		[Header("Rotation")]
		[Tooltip("This enables rotation which makes the gameobject orient to the rotation of you hand as it moves through the air. All below rotation variables have no use when this is false.")]
		[ShowIf("grabType", DistanceGrabType.Velocity)]
		public bool rotate = true;

		[Tooltip("Speed that the object orients to the rotation of your hand.")]
		[ShowIf("grabType", DistanceGrabType.Velocity)]
		public float rotationSpeed = 1f;

		[AutoToggleHeader("Enable Highlighting", 0, 0)]
		[Tooltip("Whether or not to ignore all highlights including default highlights on HandPointGrab")]
		public bool ignoreHighlights = true;

		[EnableIf("ignoreHighlights")]
		[Tooltip("Highlight targeted material to use - defaults to HandPointGrab materials if none")]
		public Material targetedMaterial;

		[EnableIf("ignoreHighlights")]
		[Tooltip("Highlight selected material to use - defaults to HandPointGrab materials if none")]
		public Material selectedMaterial;

		[AutoToggleHeader("Show Events", 0, 0)]
		public bool showEvents = true;

		[ShowIf("showEvents")]
		public UnityHandGrabEvent OnPull;

		[Space]
		[Tooltip("Called when the object has been targeted/aimed at by the pointer")]
		[ShowIf("showEvents")]
		public UnityHandGrabEvent StartTargeting;

		[ShowIf("showEvents")]
		public UnityHandGrabEvent StopTargeting;

		[Space]
		[Tooltip("Called when the object has been selected before being pulled or flicked")]
		[ShowIf("showEvents")]
		public UnityHandGrabEvent StartSelecting;

		[ShowIf("showEvents")]
		public UnityHandGrabEvent StopSelecting;

		public HandGrabEvent OnPullCanceled;

		internal Grabbable grabbable;

		private Transform target;

		private Vector3 calculatedNecessaryVelocity;

		private bool gravitationEnabled;

		private bool gravitationMethodBegun;

		private bool pullStarted;

		private Rigidbody body;

		private float timePassedSincePull;

		private Vector3 lastGravitationVelocity;

		private void Start()
		{
			grabbable = GetComponent<Grabbable>();
			Grabbable obj = grabbable;
			obj.OnGrabEvent = (HandGrabEvent)Delegate.Combine(obj.OnGrabEvent, (HandGrabEvent)delegate
			{
				gravitationEnabled = false;
			});
			body = grabbable.body;
		}

		private void FixedUpdate()
		{
			if (!instantPull && grabType == DistanceGrabType.Velocity && !(target == null))
			{
				InitialVelocityPushToHand();
				if (rotate)
				{
					FollowHandRotation();
				}
				if (gravitationEnabled)
				{
					GravitateTowardsHand();
				}
				timePassedSincePull += Time.fixedDeltaTime;
			}
		}

		private void FollowHandRotation()
		{
			base.transform.rotation = Quaternion.Slerp(base.transform.rotation, target.rotation, rotationSpeed * Time.fixedDeltaTime);
		}

		private void GravitateTowardsHand()
		{
			if (gravitationEnabled)
			{
				if (!gravitationMethodBegun)
				{
					gravitationMethodBegun = true;
				}
				lastGravitationVelocity = (target.position - base.transform.position).normalized * Time.fixedDeltaTime * gravitationVelocity;
				body.velocity += lastGravitationVelocity * 10f;
			}
			else
			{
				gravitationMethodBegun = false;
			}
		}

		private void InitialVelocityPushToHand()
		{
			if (pullStarted)
			{
				if (archMultiplier > 0f)
				{
					calculatedNecessaryVelocity = CalculateTrajectoryVelocity(base.transform.position, target.transform.position, archMultiplier);
				}
				timePassedSincePull = 0f;
				body.velocity = calculatedNecessaryVelocity;
				gravitationEnabled = true;
				pullStarted = false;
			}
		}

		private void OnCollisionEnter(Collision collision)
		{
			if (timePassedSincePull > 0.2f)
			{
				pullStarted = false;
				gravitationEnabled = false;
				CancelTarget();
			}
		}

		private Vector3 CalculateTrajectoryVelocity(Vector3 origin, Vector3 target, float t)
		{
			float x = (target.x - origin.x) / t;
			float z = (target.z - origin.z) / t;
			float y = (target.y - origin.y - 0.5f * Physics.gravity.y * t * t) / t;
			return new Vector3(x, y, z);
		}

		public void SetTarget(Transform theObject)
		{
			target = theObject;
			pullStarted = true;
		}

		public void CancelTarget()
		{
			target = null;
			pullStarted = false;
		}
	}
}
