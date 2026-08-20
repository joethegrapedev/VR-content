using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace Autohand
{
	[Serializable]
	[RequireComponent(typeof(Rigidbody))]
	[DefaultExecutionOrder(-10)]
	public class HandBase : MonoBehaviour
	{
		[AutoHeader("AUTO HAND", 0, 0)]
		public bool ignoreMe;

		public Finger[] fingers;

		[Tooltip("An empty GameObject that should be placed on the surface of the center of the palm")]
		public Transform palmTransform;

		[FormerlySerializedAs("isLeft")]
		[Tooltip("Whether this is the left (on) or right (off) hand")]
		public bool left;

		[Space]
		[Tooltip("Maximum distance for pickup")]
		[Min(0.01f)]
		public float reachDistance = 0.3f;

		[AutoToggleHeader("Enable Movement", 0, 0, tooltip = "Whether or not to enable the hand's Rigidbody Physics movement")]
		public bool enableMovement = true;

		[EnableIf("enableMovement")]
		[Tooltip("Follow target, the hand will always try to match this transforms position with rigidbody movements")]
		public Transform follow;

		[EnableIf("enableMovement")]
		[Tooltip("Returns hand to the target after this distance [helps just in case it gets stuck]")]
		[Min(0f)]
		public float maxFollowDistance = 0.5f;

		[EnableIf("enableMovement")]
		[Tooltip("Amplifier for applied velocity on released object")]
		[Min(0f)]
		public float throwPower = 1f;

		[EnableIf("enableMovement")]
		[Tooltip("Applies position interpolation to the hands to make the hands look smoother when the FPS is higher than the fixed timestep")]
		public bool enableInterpolation = true;

		[HideInInspector]
		public bool advancedFollowSettings = true;

		[AutoToggleHeader("Enable Auto Posing", 0, 0, tooltip = "Auto Posing will override Unity Animations -- This will disable all the Auto Hand IK, including animations from: finger sway, pose areas, finger bender scripts (runtime Auto Posing will still work)")]
		[Tooltip("Turn this on when you want to animate the hand or use other IK Drivers")]
		public bool enableIK = true;

		[EnableIf("enableIK")]
		[Tooltip("How much the fingers sway from the velocity")]
		public float swayStrength = 0.7f;

		[EnableIf("enableIK")]
		[Tooltip("This will offset each fingers bend (0 is no bend, 1 is full bend)")]
		public float gripOffset = 0.1f;

		[NonSerialized]
		[Tooltip("The maximum allowed velocity of the hand")]
		[Min(0f)]
		public float maxVelocity = 10f;

		[NonSerialized]
		[Tooltip("Follow target speed (Can cause jittering if turned too high - recommend increasing drag with speed)")]
		[Min(0f)]
		public float followPositionStrength = 80f;

		[NonSerialized]
		[HideInInspector]
		[Tooltip("Follow target rotation speed (Can cause jittering if turned too high - recommend increasing angular drag with speed)")]
		[Min(0f)]
		public float followRotationStrength = 100f;

		[NonSerialized]
		[HideInInspector]
		[Tooltip("After this many seconds velocity data within a 'throw window' will be tossed out. (This allows you to get only use acceeleration data from the last 'x' seconds of the throw.)")]
		public float throwVelocityExpireTime = 0.125f;

		[NonSerialized]
		[HideInInspector]
		[Tooltip("After this many seconds velocity data within a 'throw window' will be tossed out. (This allows you to get only use acceeleration data from the last 'x' seconds of the throw.)")]
		public float throwAngularVelocityExpireTime = 0.25f;

		[NonSerialized]
		[HideInInspector]
		[Tooltip("Increase for closer finger tip results / Decrease for less physics checks - The number of steps the fingers take when bending to grab something")]
		public int fingerBendSteps = 100;

		[NonSerialized]
		[HideInInspector]
		public float sphereCastRadius = 0.04f;

		[HideInInspector]
		public bool usingPoseAreas = true;

		[HideInInspector]
		public QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore;

		private Grabbable HoldingObj;

		private Grabbable _lookingAtObj;

		private Transform _moveTo;

		private Vector3 _grabPositionOffset = Vector3.zero;

		private Quaternion _grabRotationOffset = Quaternion.identity;

		private CollisionTracker _collisionTracker;

		protected GrabbablePose _grabPose;

		protected Joint heldJoint;

		protected bool grabbing;

		protected bool squeezing;

		protected bool grabbed;

		protected float triggerPoint;

		protected Coroutine handAnimateRoutine;

		protected HandPoseArea handPoseArea;

		protected HandPoseData preHandPoseAreaPose;

		protected List<Collider> handColliders = new List<Collider>();

		private Transform _grabPoint;

		private Transform _grabPosition;

		internal int handLayers;

		protected Collider palmCollider;

		protected RaycastHit highlightHit;

		protected HandVelocityTracker velocityTracker;

		protected Transform palmChild;

		protected Vector3 lastFrameFollowPos;

		protected Quaternion lastFrameFollowRot;

		protected Vector3 followVel;

		protected Vector3 followAngularVel;

		internal bool allowUpdateMovement = true;

		private Vector3[] handRays = new Vector3[0];

		private RaycastHit[] rayHits = new RaycastHit[0];

		private Vector3[] updatePositionTracked = new Vector3[3];

		private List<RaycastHit> closestHits = new List<RaycastHit>();

		private List<Grabbable> closestGrabs = new List<Grabbable>();

		private int tryMaxDistanceCount;

		private Vector3 lastFollowPosition;

		private Vector3 lastFollowRotation;

		private bool prerendered;

		private Vector3 preRenderPos;

		private Quaternion preRenderRot;

		private float currGrip = 1f;

		private float lastUpdateTime;

		protected bool ignoreMoveFrame;

		private float fingerSwayVel;

		public Grabbable holdingObj
		{
			get
			{
				return HoldingObj;
			}
			internal set
			{
				HoldingObj = value;
			}
		}

		public Grabbable lookingAtObj
		{
			get
			{
				return _lookingAtObj;
			}
			protected set
			{
				_lookingAtObj = value;
			}
		}

		public Transform moveTo
		{
			get
			{
				if (!base.gameObject.activeInHierarchy)
				{
					return null;
				}
				if (_moveTo == null)
				{
					_moveTo = new GameObject().transform;
					_moveTo.parent = base.transform.parent;
					_moveTo.name = "HAND FOLLOW POINT";
				}
				return _moveTo;
			}
		}

		public Rigidbody body { get; internal set; }

		public Vector3 grabPositionOffset
		{
			get
			{
				return _grabPositionOffset;
			}
			protected set
			{
				_grabPositionOffset = value;
			}
		}

		public Quaternion grabRotationOffset
		{
			get
			{
				return _grabRotationOffset;
			}
			protected set
			{
				_grabRotationOffset = value;
			}
		}

		public bool disableIK
		{
			get
			{
				return !enableIK;
			}
			set
			{
				enableIK = !value;
			}
		}

		public CollisionTracker collisionTracker
		{
			get
			{
				if (_collisionTracker == null)
				{
					_collisionTracker = base.gameObject.AddComponent<CollisionTracker>();
				}
				return _collisionTracker;
			}
			protected set
			{
				if (_collisionTracker != null)
				{
					UnityEngine.Object.Destroy(_collisionTracker);
				}
				_collisionTracker = value;
			}
		}

		protected GrabbablePose grabPose
		{
			get
			{
				return _grabPose;
			}
			set
			{
				if (value == null && _grabPose != null)
				{
					_grabPose.CancelHandPose(this as Hand);
				}
				_grabPose = value;
			}
		}

		internal Transform grabPoint
		{
			get
			{
				if (_grabPoint == null && base.gameObject.scene.isLoaded)
				{
					_grabPoint = new GameObject().transform;
					_grabPoint.name = "grabPoint";
				}
				return _grabPoint;
			}
		}

		internal Transform grabPosition
		{
			get
			{
				if (!base.gameObject.activeInHierarchy)
				{
					_grabPosition = null;
				}
				else if (base.gameObject.activeInHierarchy && _grabPosition == null)
				{
					_grabPosition = new GameObject().transform;
					_grabPosition.name = "grabPosition";
					_grabPosition.parent = base.transform;
				}
				return _grabPosition;
			}
		}

		protected virtual void Awake()
		{
			body = GetComponent<Rigidbody>();
			body.interpolation = RigidbodyInterpolation.None;
			body.useGravity = false;
			body.maxDepenetrationVelocity = 2f;
			body.solverIterations = 200;
			body.solverVelocityIterations = 200;
			if (palmCollider == null)
			{
				palmCollider = palmTransform.gameObject.AddComponent<BoxCollider>();
				(palmCollider as BoxCollider).size = new Vector3(0.2f, 0.15f, 0.05f);
				(palmCollider as BoxCollider).center = new Vector3(0f, 0f, -0.025f);
				palmCollider.enabled = false;
			}
			if (palmChild == null)
			{
				palmChild = new GameObject().transform;
				palmChild.parent = palmTransform;
				SetPalmRays();
			}
			Camera[] array = UnityEngine.Object.FindObjectsOfType<Camera>(includeInactive: true);
			for (int i = 0; i < array.Length; i++)
			{
				array[i].gameObject.AddComponent<HandStabilizer>().hand = this;
			}
			if (velocityTracker == null)
			{
				velocityTracker = new HandVelocityTracker(this);
			}
		}

		protected virtual void OnEnable()
		{
			SetHandCollidersRecursive(base.transform);
		}

		protected virtual void OnDisable()
		{
			handColliders.Clear();
		}

		protected virtual void OnDestroy()
		{
			if (grabPoint != null)
			{
				UnityEngine.Object.Destroy(grabPoint.gameObject);
			}
			if (grabPosition != null)
			{
				UnityEngine.Object.Destroy(grabPosition.gameObject);
			}
			if (moveTo != null)
			{
				UnityEngine.Object.Destroy(moveTo.gameObject);
			}
		}

		protected virtual void FixedUpdate()
		{
			if (!IsGrabbing() && enableMovement && follow != null && !body.isKinematic)
			{
				MoveTo();
				TorqueTo();
			}
			velocityTracker.UpdateThrowing();
			if (ignoreMoveFrame)
			{
				body.velocity = Vector3.zero;
				body.angularVelocity = Vector3.zero;
			}
			if (follow != null)
			{
				followVel = follow.position - lastFollowPosition;
				followAngularVel = follow.rotation.eulerAngles - lastFollowPosition;
				lastFollowPosition = follow.position;
				lastFollowRotation = follow.rotation.eulerAngles;
			}
			lastUpdateTime = Time.fixedTime;
			ignoreMoveFrame = false;
		}

		protected virtual void Update()
		{
			if (CollisionCount() == 0 && enableMovement && follow != null && !body.isKinematic && !IsGrabbing() && enableInterpolation)
			{
				float num = Time.time - lastUpdateTime;
				Vector3 position = base.transform.position;
				if (holdingObj == null)
				{
					MoveTo();
					body.velocity *= 1f - Mathf.Clamp01(body.drag * num);
					base.transform.position = Vector3.MoveTowards(base.transform.position, moveTo.position, body.velocity.magnitude * num);
					body.position = base.transform.position;
					TorqueTo();
					body.angularVelocity *= 1f - Mathf.Clamp01(body.angularDrag * num);
					base.transform.rotation = Quaternion.Euler(Vector3.MoveTowards(base.transform.rotation.eulerAngles, moveTo.rotation.eulerAngles, body.angularVelocity.magnitude * num));
					body.rotation = base.transform.rotation;
					Vector3 vector = base.transform.position - position;
					if (base.transform.position != position && body.SweepTest(vector, out var _, vector.magnitude))
					{
						base.transform.position -= vector;
						body.position = base.transform.position;
					}
				}
				else if (holdingObj.ShouldInterpolate())
				{
					Vector3 position2 = grabPosition.position;
					Quaternion rotation = grabPosition.rotation;
					MoveTo();
					body.velocity *= 1f - body.drag * num;
					base.transform.position = Vector3.MoveTowards(base.transform.position, moveTo.position, body.velocity.magnitude * num);
					body.position = base.transform.position;
					TorqueTo();
					body.angularVelocity *= 1f - body.angularDrag * num;
					base.transform.rotation = Quaternion.Euler(Vector3.MoveTowards(base.transform.rotation.eulerAngles, moveTo.rotation.eulerAngles, body.angularVelocity.magnitude * num));
					body.rotation = base.transform.rotation;
					Vector3 vector2 = base.transform.position - position;
					Vector3 direction = grabPosition.position - position2;
					if (base.transform.position != position)
					{
						bool flag = false;
						RaycastHit[] array = holdingObj.body.SweepTestAll(direction, direction.magnitude);
						for (int i = 0; i < array.Length; i++)
						{
							if (array[i].rigidbody != holdingObj.body && !holdingObj.IsHolding(array[i].rigidbody))
							{
								flag = true;
							}
						}
						if (flag)
						{
							base.transform.position -= vector2;
							body.position = base.transform.position;
						}
						else
						{
							array = holdingObj.body.SweepTestAll(direction, direction.magnitude);
							for (int j = 0; j < array.Length; j++)
							{
								if (array[j].rigidbody != holdingObj.body && !holdingObj.IsHolding(array[j].rigidbody))
								{
									flag = true;
								}
							}
						}
						if (flag)
						{
							base.transform.position -= base.transform.position - position;
							body.position = base.transform.position;
						}
						else
						{
							holdingObj.body.transform.position += grabPosition.position - position2;
							holdingObj.body.transform.rotation *= grabPosition.rotation * Quaternion.Inverse(rotation);
							holdingObj.body.position = holdingObj.body.transform.position;
							holdingObj.body.rotation = holdingObj.body.transform.rotation;
						}
					}
				}
			}
			if (!IsGrabbing())
			{
				UpdateFingers(Time.deltaTime);
			}
			for (int k = 1; k < updatePositionTracked.Length; k++)
			{
				updatePositionTracked[k] = updatePositionTracked[k - 1];
			}
			updatePositionTracked[0] = base.transform.localPosition;
			lastUpdateTime = Time.time;
		}

		public virtual void OnPreRender()
		{
			preRenderPos = base.transform.position;
			preRenderRot = base.transform.rotation;
			if (holdingObj != null && holdingObj.customGrabJoint == null && !IsGrabbing())
			{
				base.transform.position = grabPoint.position;
				base.transform.rotation = grabPoint.rotation;
				prerendered = true;
			}
		}

		public virtual void OnPostRender()
		{
			if (holdingObj != null && holdingObj.customGrabJoint == null && !IsGrabbing() && prerendered)
			{
				base.transform.position = preRenderPos;
				base.transform.rotation = preRenderRot;
			}
			prerendered = false;
		}

		protected virtual void CreateJoint(Grabbable grab, float breakForce, float breakTorque)
		{
			if (grab.customGrabJoint == null)
			{
				FixedJoint copyOf = base.gameObject.AddComponent<FixedJoint>().GetCopyOf(Resources.Load<FixedJoint>("DefaultJoint"));
				copyOf.anchor = Vector3.zero;
				copyOf.breakForce = breakForce;
				if (grab.HeldCount() == 1)
				{
					copyOf.breakForce += 250f;
				}
				copyOf.breakTorque = breakTorque;
				copyOf.connectedBody = grab.body;
				heldJoint = copyOf;
			}
			else
			{
				ConfigurableJoint copyOf2 = grab.body.gameObject.AddComponent<ConfigurableJoint>().GetCopyOf(grab.customGrabJoint);
				copyOf2.anchor = Vector3.zero;
				if (grab.HeldCount() == 1)
				{
					copyOf2.breakForce += 250f;
				}
				copyOf2.breakForce = breakForce;
				copyOf2.breakTorque = breakTorque;
				copyOf2.connectedBody = body;
				heldJoint = copyOf2;
			}
		}

		protected virtual void MoveTo()
		{
			SetMoveTo();
			if (followPositionStrength <= 0f)
			{
				return;
			}
			Vector3 position = moveTo.position;
			float num = Vector3.Distance(position, base.transform.position);
			if (num > maxFollowDistance)
			{
				if (holdingObj != null)
				{
					if (holdingObj.parentOnGrab && tryMaxDistanceCount < 1)
					{
						SetHandLocation(position, base.transform.rotation);
						tryMaxDistanceCount += 2;
					}
					else if (!holdingObj.parentOnGrab || tryMaxDistanceCount >= 1)
					{
						holdingObj.ForceHandRelease(this as Hand);
						SetHandLocation(position, base.transform.rotation);
					}
				}
				else
				{
					SetHandLocation(position, base.transform.rotation);
				}
			}
			if (tryMaxDistanceCount > 0)
			{
				tryMaxDistanceCount--;
			}
			if ((bool)holdingObj && holdingObj.HeldCount() > 1)
			{
				num = Mathf.Clamp(num, 0f, 0.04f);
			}
			if (CollisionCount() > 0)
			{
				float num2 = maxVelocity;
				Vector3 vector = (position - base.transform.position).normalized * followPositionStrength * num;
				vector.x = Mathf.Clamp(vector.x, 0f - num2, num2);
				vector.y = Mathf.Clamp(vector.y, 0f - num2, num2);
				vector.z = Mathf.Clamp(vector.z, 0f - num2, num2);
				if (!holdingObj || holdingObj.HeldCount() <= 1)
				{
					vector = Vector3.MoveTowards(body.velocity, vector, 0.6f + body.velocity.magnitude / num2 * (60f / Time.fixedDeltaTime));
				}
				body.velocity = vector;
			}
			else
			{
				float num3 = maxVelocity;
				Vector3 velocity = (position - base.transform.position).normalized * followPositionStrength * num;
				velocity.x = Mathf.Clamp(velocity.x, 0f - num3, num3);
				velocity.y = Mathf.Clamp(velocity.y, 0f - num3, num3);
				velocity.z = Mathf.Clamp(velocity.z, 0f - num3, num3);
				body.velocity = velocity;
			}
		}

		protected virtual void TorqueTo()
		{
			(moveTo.rotation * Quaternion.Inverse(body.rotation)).ToAngleAxis(out var angle, out var axis);
			if (!float.IsInfinity(axis.x))
			{
				if (angle > 180f)
				{
					angle -= 360f;
				}
				Vector3 b = MathF.PI / 180f * angle * followRotationStrength * axis.normalized;
				if (CollisionCount() > 0)
				{
					body.angularVelocity = Vector3.Lerp(body.angularVelocity, b, 0.65f);
				}
				else
				{
					body.angularVelocity = Vector3.Lerp(body.angularVelocity, b, 0.8f);
				}
			}
		}

		public virtual void SetHandLocation(Vector3 pos, Quaternion rot)
		{
			if ((bool)holdingObj && holdingObj.parentOnGrab)
			{
				holdingObj.IgnoreInterpolationForOneFixedUpdate();
				ignoreMoveFrame = true;
				Vector3 vector = pos - base.transform.position;
				Quaternion quaternion = rot * Quaternion.Inverse(base.transform.rotation);
				base.transform.position = pos;
				base.transform.rotation = rot;
				body.position = base.transform.position;
				body.rotation = base.transform.rotation;
				Quaternion rotation = holdingObj.body.transform.rotation;
				holdingObj.body.transform.position += vector;
				holdingObj.body.transform.RotateAround(base.transform, quaternion);
				holdingObj.body.position = holdingObj.body.transform.position;
				holdingObj.body.rotation = holdingObj.body.transform.rotation;
				grabPositionOffset = quaternion * grabPositionOffset;
				Quaternion deltaRotation = Quaternion.Inverse(holdingObj.body.rotation) * rotation;
				foreach (Rigidbody jointedBody in holdingObj.jointedBodies)
				{
					if (!jointedBody.CanGetComponent<Grabbable>(out var component) || component.HeldCount() <= 0)
					{
						jointedBody.position += vector;
						jointedBody.transform.RotateAround(holdingObj.body.transform, deltaRotation);
					}
				}
				velocityTracker.ClearThrow();
			}
			else
			{
				ignoreMoveFrame = true;
				base.transform.position = pos;
				base.transform.rotation = rot;
				body.position = pos;
				body.rotation = rot;
				body.velocity = Vector3.zero;
				body.angularVelocity = Vector3.zero;
			}
		}

		public virtual void SetHandLocation(Vector3 pos)
		{
			SetMoveTo();
			SetHandLocation(pos, base.transform.rotation);
		}

		public void ResetHandLocation()
		{
			SetHandLocation(moveTo.position, moveTo.rotation);
		}

		protected void SetMoveTo()
		{
			if (follow == null)
			{
				return;
			}
			moveTo.position = follow.position;
			moveTo.rotation = follow.rotation;
			if (holdingObj != null)
			{
				moveTo.position = follow.position + grabPositionOffset;
				moveTo.rotation = follow.rotation * grabRotationOffset;
				if (left)
				{
					Vector3 euler = -holdingObj.heldRotationOffset;
					euler.x *= -1f;
					moveTo.localRotation *= Quaternion.Euler(euler);
					Vector3 heldPositionOffset = holdingObj.heldPositionOffset;
					heldPositionOffset.x *= -1f;
					moveTo.position += base.transform.rotation * heldPositionOffset;
				}
				else
				{
					moveTo.position += base.transform.rotation * holdingObj.heldPositionOffset;
					moveTo.localRotation *= Quaternion.Euler(holdingObj.heldRotationOffset);
				}
			}
		}

		public bool CanGrab(Grabbable grab)
		{
			bool flag = grab.IsHeld() && grab.singleHandOnly && !grab.allowHeldSwapping;
			if (grab.CanGrab(this) && !IsGrabbing())
			{
				return !flag;
			}
			return false;
		}

		public float GetTriggerAxis()
		{
			return triggerPoint;
		}

		protected virtual Vector3 HandClosestHit(out RaycastHit closestHit, out Grabbable grabbable, float dist, int layerMask, Grabbable target = null)
		{
			Vector3 forward = palmTransform.forward;
			Vector3 position = palmTransform.position;
			Quaternion rotation = palmTransform.rotation;
			Grabbable grabbable2 = null;
			closestGrabs.Clear();
			closestHits.Clear();
			for (int i = 0; i < handRays.Length; i++)
			{
				if (!Physics.SphereCast(position - forward * sphereCastRadius, sphereCastRadius, rotation * handRays[i], out rayHits[i], dist, layerMask, queryTriggerInteraction) || (queryTriggerInteraction != QueryTriggerInteraction.Collide && rayHits[i].collider.isTrigger) || !(palmTransform.InverseTransformPoint(rayHits[i].point).z > 0f))
				{
					continue;
				}
				GameObject gameObject = rayHits[i].collider.gameObject;
				if (closestGrabs.Count > 0)
				{
					grabbable2 = closestGrabs[closestGrabs.Count - 1];
				}
				Grabbable grabbable3;
				if (closestGrabs.Count > 0 && gameObject == grabbable2.gameObject)
				{
					if (target == null)
					{
						closestGrabs.Add(grabbable2);
						closestHits.Add(rayHits[i]);
					}
				}
				else if (gameObject.HasGrabbable(out grabbable3) && CanGrab(grabbable3) && (target == null || target == grabbable3))
				{
					closestGrabs.Add(grabbable3);
					closestHits.Add(rayHits[i]);
				}
			}
			int count = closestHits.Count;
			if (count > 0)
			{
				closestHit = closestHits[0];
				grabbable = closestGrabs[0];
				Vector3 zero = Vector3.zero;
				for (int j = 0; j < count; j++)
				{
					if (closestHits[j].distance / closestGrabs[j].grabPriorityWeight < closestHit.distance / grabbable.grabPriorityWeight)
					{
						closestHit = closestHits[j];
						grabbable = closestGrabs[j];
					}
					zero += closestHits[j].point - palmTransform.position;
				}
				if (holdingObj == null && !IsGrabbing())
				{
					if (grabPoint.parent != closestHit.transform)
					{
						grabPoint.parent = closestHit.transform;
					}
					grabPoint.position = closestHit.point;
					grabPoint.up = closestHit.normal;
				}
				return zero / count;
			}
			closestHit = default(RaycastHit);
			grabbable = null;
			return Vector3.zero;
		}

		public bool IsPosing()
		{
			if (!(handPoseArea != null) && (!(holdingObj != null) || !holdingObj.HasCustomPose()))
			{
				return handAnimateRoutine != null;
			}
			return true;
		}

		protected virtual void UpdateFingers(float deltaTime)
		{
			Vector3 zero = Vector3.zero;
			for (int i = 1; i < updatePositionTracked.Length; i++)
			{
				zero += updatePositionTracked[i] - updatePositionTracked[i - 1];
			}
			zero /= (float)updatePositionTracked.Length;
			zero = Quaternion.Inverse(palmTransform.rotation) * base.transform.parent.rotation * zero;
			if (!grabbing && !disableIK && !IsPosing() && !holdingObj)
			{
				float num = (zero * 60f).z;
				if (CollisionCount() > 0)
				{
					num = 0f;
				}
				fingerSwayVel = Mathf.MoveTowards(fingerSwayVel, num, deltaTime * Mathf.Abs((fingerSwayVel - num) * 30f));
				float bend = (currGrip = gripOffset + swayStrength * fingerSwayVel);
				Finger[] array = fingers;
				for (int j = 0; j < array.Length; j++)
				{
					array[j].UpdateFinger(bend);
				}
			}
		}

		public int CollisionCount()
		{
			if (holdingObj != null)
			{
				return collisionTracker.collisionObjects.Count + holdingObj.collisionTracker.collisionObjects.Count;
			}
			return collisionTracker.collisionObjects.Count;
		}

		public void HandIgnoreCollider(Collider collider, bool ignore)
		{
			for (int i = 0; i < handColliders.Count; i++)
			{
				Physics.IgnoreCollision(handColliders[i], collider, ignore);
			}
		}

		public void SetLayer()
		{
			SetLayerRecursive(base.transform, LayerMask.NameToLayer(left ? Hand.leftHandLayerName : Hand.rightHandLayerName));
		}

		internal void SetLayerRecursive(Transform obj, int newLayer)
		{
			obj.gameObject.layer = newLayer;
			for (int i = 0; i < obj.childCount; i++)
			{
				SetLayerRecursive(obj.GetChild(i), newLayer);
			}
		}

		protected void SetHandCollidersRecursive(Transform obj)
		{
			handColliders.Clear();
			AddHandCol(obj);
			void AddHandCol(Transform transform)
			{
				Collider[] components = transform.GetComponents<Collider>();
				foreach (Collider item in components)
				{
					handColliders.Add(item);
				}
				for (int j = 0; j < transform.childCount; j++)
				{
					AddHandCol(transform.GetChild(j));
				}
			}
		}

		public Vector3[] GetPalmRays()
		{
			SetPalmRays();
			return handRays;
		}

		protected virtual void SetPalmRays()
		{
			List<Vector3> list = new List<Vector3>();
			for (int i = 0; i < 100; i++)
			{
				float num = Mathf.Sqrt(Mathf.Clamp((float)i * 525f, 0.0001f, float.MaxValue)) / MathF.PI;
				list.Add(Quaternion.Euler(0f, Mathf.Cos((float)i * 0.98429203f) * num + 90f, Mathf.Sin((float)i * 0.98429203f) * num) * -Vector3.right);
			}
			rayHits = new RaycastHit[100];
			handRays = list.ToArray();
		}

		public Vector3 ThrowVelocity()
		{
			return velocityTracker.ThrowVelocity();
		}

		public Vector3 ThrowAngularVelocity()
		{
			return velocityTracker.ThrowAngularVelocity();
		}

		public bool IsGrabbing()
		{
			return grabbing;
		}

		public static int GetHandsLayerMask()
		{
			return LayerMask.GetMask(Hand.rightHandLayerName, Hand.leftHandLayerName);
		}
	}
}
