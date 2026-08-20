using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace Autohand
{
	[RequireComponent(typeof(Rigidbody))]
	[RequireComponent(typeof(CapsuleCollider))]
	[DefaultExecutionOrder(-30)]
	[HelpURL("https://earnestrobot.notion.site/Auto-Move-Player-02d91305a4294e039049bd45cacc5b90")]
	public class AutoHandPlayer : MonoBehaviour
	{
		private static bool notFound;

		public static AutoHandPlayer _Instance;

		[AutoHeader("Auto Hand Player", 0, 0)]
		public bool ignoreMe;

		[Tooltip("The tracked headCamera object")]
		public Camera headCamera;

		[Tooltip("The object that represents the forward direction movement, usually should be set as the camera or a tracked controller")]
		public Transform forwardFollow;

		[Tooltip("This should NOT be a child of this body. This should be a GameObject that contains all the tracked objects (head/controllers)")]
		public Transform trackingContainer;

		public Hand handRight;

		public Hand handLeft;

		[AutoToggleHeader("Movement", 0, 0)]
		public bool useMovement = true;

		[EnableIf("useMovement")]
		[FormerlySerializedAs("moveSpeed")]
		[Tooltip("Movement speed when isGrounded")]
		public float maxMoveSpeed = 1.5f;

		[EnableIf("useMovement")]
		[Tooltip("Movement acceleration when isGrounded")]
		public float moveAcceleration = 10f;

		[EnableIf("useMovement")]
		[Tooltip("Movement acceleration when isGrounded")]
		public float groundedDrag = 4f;

		[AutoToggleHeader("Snap Turning", 0, 0)]
		[Tooltip("Whether or not to use snap turning or smooth turning")]
		[Min(0f)]
		public bool snapTurning = true;

		[Tooltip("turn speed when not using snap turning - if snap turning, represents angle per snap")]
		[ShowIf("snapTurning")]
		public float snapTurnAngle = 30f;

		[HideIf("snapTurning")]
		public float smoothTurnSpeed = 10f;

		[AutoToggleHeader("Height", 0, 0)]
		public bool showHeight = true;

		[ShowIf("showHeight")]
		public float heightSmoothSpeed = 20f;

		[ShowIf("showHeight")]
		public float heightOffset;

		[ShowIf("showHeight")]
		public bool crouching;

		[ShowIf("showHeight")]
		public float crouchHeight = 0.6f;

		[ShowIf("showHeight")]
		[Tooltip("Whether or not the capsule height should be adjusted to match the headCamera height")]
		public bool autoAdjustColliderHeight = true;

		[ShowIf("showHeight")]
		[Tooltip("Minimum and maximum auto adjusted height, to adjust height without auto adjustment change capsule collider height instead")]
		public Vector2 minMaxHeight = new Vector2(0.5f, 2.5f);

		[ShowIf("showHeight")]
		public bool useHeadCollision = true;

		[ShowIf("showHeight")]
		public float headRadius = 0.15f;

		[AutoToggleHeader("Use Grounding", 0, 0)]
		public bool useGrounding = true;

		[EnableIf("useGrounding")]
		[Tooltip("Maximum height that the body can step up onto")]
		[Min(0f)]
		public float maxStepHeight = 0.3f;

		[EnableIf("useGrounding")]
		[Tooltip("Maximum angle the player can walk on")]
		[Min(0f)]
		public float maxStepAngle = 30f;

		[EnableIf("useGrounding")]
		[Tooltip("The layers that count as ground")]
		public LayerMask groundLayerMask;

		[AutoToggleHeader("Enable Climbing", 0, 0)]
		[Tooltip("Whether or not the player can use Climbable objects  (Objects with the Climbable component)")]
		public bool allowClimbing = true;

		[Tooltip("Whether or not the player move while climbing")]
		[ShowIf("allowClimbing")]
		public bool allowClimbingMovement = true;

		[Tooltip("How quickly the player can climb")]
		[ShowIf("allowClimbing")]
		public Vector3 climbingStrength = new Vector3(20f, 20f, 20f);

		public float climbingAcceleration = 30f;

		public float climbingDrag = 5f;

		[Tooltip("Inscreases the step height while climbing up to make it easier to step up onto a surface")]
		public float climbUpStepHeightMultiplier = 1f;

		[AutoToggleHeader("Enable Pushing", 0, 0)]
		[Tooltip("Whether or not the player can use Pushable objects (Objects with the Pushable component)")]
		public bool allowBodyPushing = true;

		[Tooltip("How quickly the player can climb")]
		[EnableIf("allowBodyPushing")]
		public Vector3 pushingStrength = new Vector3(10f, 10f, 10f);

		public float pushingAcceleration = 10f;

		public float pushingDrag = 3f;

		[Tooltip("Inscreases the step height while pushing up to make it easier to step up onto a surface")]
		public float pushUpStepHeightMultiplier = 1f;

		[AutoToggleHeader("Enable Platforming", 0, 0)]
		[Tooltip("Platforms will move the player with them. A platform is an object with the Transform component on it")]
		public bool allowPlatforms = true;

		[EnableIf("useGrounding")]
		[Tooltip("The layers that platforming will be enabled on, will not work with layers that the HandPlayer can't collide with")]
		public LayerMask platformingLayerMask = -1;

		private float movementDeadzone = 0.1f;

		private float turnDeadzone = 0.4f;

		public const string HandPlayerLayer = "HandPlayer";

		private const int groundRayCount = 21;

		private float turnResetzone = 0.3f;

		private float groundedOffset = 0.05f;

		private bool tempDisableGrounding;

		private HeadPhysicsFollower headPhysicsFollower;

		private CapsuleCollider bodyCapsule;

		private Vector3 moveDirection;

		private float turningAxis;

		private bool isGrounded;

		private bool axisReset = true;

		private float playerHeight;

		private bool lastCrouching;

		private float lastCrouchingHeight;

		private Vector3 targetTrackedPos;

		private Vector3 lastUpdatePosition;

		private bool editorSelected;

		private Hand lastRightHand;

		private Hand lastLeftHand;

		private Vector3 climbAxis;

		private Dictionary<Hand, Climbable> climbing = new Dictionary<Hand, Climbable>();

		private Dictionary<Pushable, Hand> pushRight = new Dictionary<Pushable, Hand>();

		private Dictionary<Pushable, int> pushRightCount = new Dictionary<Pushable, int>();

		private Dictionary<Pushable, Hand> pushLeft = new Dictionary<Pushable, Hand>();

		private Dictionary<Pushable, int> pushLeftCount = new Dictionary<Pushable, int>();

		private Vector3 pushAxis;

		private Vector3 lastPlatformPosition;

		private Quaternion lastPlatformRotation;

		private RaycastHit closestHit;

		private float lastUpdateTime;

		private bool ignoreIterpolationFrame;

		private Vector3 targetPosOffset;

		private int handPlayerMask;

		private bool trackingStarted;

		private Vector3 lastHeadPos;

		private Vector3 offset;

		private RaycastHit newClosestHit;

		private float highestPoint;

		private Coroutine disableGroundingRoutine;

		public static AutoHandPlayer Instance
		{
			get
			{
				if (_Instance == null && !notFound)
				{
					_Instance = UnityEngine.Object.FindObjectOfType<AutoHandPlayer>();
				}
				if (_Instance == null)
				{
					notFound = true;
				}
				return _Instance;
			}
		}

		public CapsuleCollider bodyCollider => bodyCapsule;

		public Rigidbody body { get; private set; }

		public virtual void Start()
		{
			lastUpdatePosition = base.transform.position;
			base.gameObject.layer = LayerMask.NameToLayer("HandPlayer");
			bodyCapsule = GetComponent<CapsuleCollider>();
			body = GetComponent<Rigidbody>();
			body.interpolation = RigidbodyInterpolation.None;
			body.freezeRotation = true;
			if (body.collisionDetectionMode == CollisionDetectionMode.Discrete)
			{
				body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
			}
			if (forwardFollow == null)
			{
				forwardFollow = headCamera.transform;
			}
			targetTrackedPos = trackingContainer.position;
			if (useHeadCollision)
			{
				CreateHeadFollower();
			}
			StartCoroutine(CheckForTrackingStart());
			handPlayerMask = AutoHandExtensions.GetPhysicsLayerMask(base.gameObject.layer);
		}

		protected virtual void OnEnable()
		{
			EnableHand(handRight);
			EnableHand(handLeft);
		}

		protected virtual void OnDisable()
		{
			DisableHand(handRight);
			DisableHand(handLeft);
		}

		private IEnumerator CheckForTrackingStart()
		{
			yield return new WaitForEndOfFrame();
			yield return new WaitForFixedUpdate();
			lastHeadPos = headCamera.transform.position;
			while (!trackingStarted)
			{
				if (headCamera.transform.position != lastHeadPos)
				{
					trackingStarted = true;
				}
				lastHeadPos = headCamera.transform.position;
				yield return new WaitForEndOfFrame();
			}
		}

		protected virtual void OnHeadTrackingStarted()
		{
			SetPosition(base.transform.position);
		}

		private void CreateHeadFollower()
		{
			if (headPhysicsFollower == null)
			{
				Transform transform = new GameObject().transform;
				transform.transform.position = headCamera.transform.position;
				transform.name = "Head Follower";
				transform.parent = base.transform.parent;
				SphereCollider sphereCollider = transform.gameObject.AddComponent<SphereCollider>();
				sphereCollider.material = bodyCapsule.material;
				sphereCollider.radius = bodyCapsule.radius;
				Rigidbody rigidbody = transform.gameObject.AddComponent<Rigidbody>();
				rigidbody.drag = 5f;
				rigidbody.angularDrag = 5f;
				rigidbody.freezeRotation = false;
				rigidbody.mass = body.mass / 3f;
				headPhysicsFollower = transform.gameObject.AddComponent<HeadPhysicsFollower>();
				headPhysicsFollower.headCamera = headCamera;
				headPhysicsFollower.followBody = base.transform;
				headPhysicsFollower.trackingContainer = trackingContainer;
			}
		}

		private void CheckHands()
		{
			if (lastLeftHand != handLeft)
			{
				EnableHand(handLeft);
				lastLeftHand = handLeft;
			}
			if (lastRightHand != handRight)
			{
				EnableHand(handRight);
				lastRightHand = handRight;
			}
		}

		private void EnableHand(Hand hand)
		{
			hand.OnGrabbed += OnHandGrab;
			hand.OnReleased += OnHandRelease;
			if (allowClimbing)
			{
				hand.OnGrabbed += StartClimb;
				hand.OnReleased += EndClimb;
			}
			if (allowBodyPushing)
			{
				hand.OnGrabbed += StartGrabPush;
				hand.OnReleased += EndGrabPush;
				hand.OnHandCollisionStart += StartPush;
				hand.OnHandCollisionStop += StopPush;
			}
		}

		private void DisableHand(Hand hand)
		{
			hand.OnGrabbed -= OnHandGrab;
			hand.OnReleased -= OnHandRelease;
			if (allowClimbing)
			{
				hand.OnGrabbed -= StartClimb;
				hand.OnReleased -= EndClimb;
				if (climbing.ContainsKey(hand))
				{
					climbing.Remove(hand);
				}
			}
			if (allowBodyPushing)
			{
				hand.OnGrabbed -= StartGrabPush;
				hand.OnReleased -= EndGrabPush;
				hand.OnHandCollisionStart -= StartPush;
				hand.OnHandCollisionStop -= StopPush;
				if (hand.left)
				{
					pushLeft.Clear();
					pushLeftCount.Clear();
				}
				else
				{
					pushRight.Clear();
					pushRightCount.Clear();
				}
			}
		}

		private void OnHandGrab(Hand hand, Grabbable grab)
		{
			grab.IgnoreColliders(bodyCapsule);
			if (headPhysicsFollower != null)
			{
				grab?.IgnoreColliders(headPhysicsFollower.headCollider);
			}
		}

		private void OnHandRelease(Hand hand, Grabbable grab)
		{
			if (grab != null && grab.HeldCount() == 0)
			{
				grab?.IgnoreColliders(bodyCapsule, ignore: false);
				if (headPhysicsFollower != null)
				{
					grab?.IgnoreColliders(headPhysicsFollower.headCollider, ignore: false);
				}
				if ((bool)grab && grab.parentOnGrab && grab.body != null)
				{
					grab.body.velocity += body.velocity / 2f;
				}
			}
		}

		public void IgnoreCollider(Collider col, bool ignore)
		{
			Physics.IgnoreCollision(bodyCapsule, col, ignore);
			Physics.IgnoreCollision(headPhysicsFollower.headCollider, col, ignore);
		}

		public virtual void Move(Vector2 axis, bool useDeadzone = true, bool useRelativeDirection = false)
		{
			moveDirection.x = ((!useDeadzone || Mathf.Abs(axis.x) > movementDeadzone) ? axis.x : 0f);
			moveDirection.z = ((!useDeadzone || Mathf.Abs(axis.y) > movementDeadzone) ? axis.y : 0f);
			if (useRelativeDirection)
			{
				moveDirection = base.transform.rotation * moveDirection;
			}
		}

		public virtual void Turn(float turnAxis)
		{
			turnAxis = ((Mathf.Abs(turnAxis) > turnDeadzone) ? turnAxis : 0f);
			turningAxis = turnAxis;
		}

		private void Update()
		{
			if (useMovement)
			{
				UpdatePlatform(isFixedUpdate: false);
				InterpolateMovement();
				UpdateTurn(Time.deltaTime);
			}
		}

		protected virtual void FixedUpdate()
		{
			CheckHands();
			UpdatePlayerHeight();
			if (useMovement)
			{
				ApplyPushingForce();
				ApplyClimbingForce();
				Ground();
				UpdateRigidbody(moveDirection);
				UpdatePlatform(isFixedUpdate: true);
				UpdateTurn(Time.fixedDeltaTime);
			}
		}

		protected virtual void UpdateRigidbody(Vector3 moveDir)
		{
			Vector3 vector = AlterDirection(moveDir);
			float y = body.velocity.y;
			if (pushAxis != Vector3.zero)
			{
				body.velocity = Vector3.MoveTowards(body.velocity, pushAxis, pushingAcceleration * Time.fixedDeltaTime);
				body.velocity *= 1f - Mathf.Clamp01(pushingDrag * Time.fixedDeltaTime);
			}
			if (climbAxis != Vector3.zero)
			{
				body.velocity = Vector3.MoveTowards(body.velocity, climbAxis, climbingAcceleration * Time.fixedDeltaTime);
				body.velocity *= 1f - Mathf.Clamp01(climbingDrag * Time.fixedDeltaTime);
			}
			if (vector != Vector3.zero && CanInputMove())
			{
				Vector3 velocity = Vector3.MoveTowards(body.velocity, vector * maxMoveSpeed, moveAcceleration * Time.fixedDeltaTime);
				if (vector.x < 0f)
				{
					if (body.velocity.x > 0f - maxMoveSpeed && velocity.x <= 0f - maxMoveSpeed)
					{
						velocity.x = 0f - maxMoveSpeed;
					}
					else if (body.velocity.x < 0f - maxMoveSpeed)
					{
						velocity.x = body.velocity.x;
					}
				}
				else if (body.velocity.x < maxMoveSpeed && velocity.x >= maxMoveSpeed)
				{
					velocity.x = maxMoveSpeed;
				}
				else if (body.velocity.x > maxMoveSpeed)
				{
					velocity.x = body.velocity.x;
				}
				if (vector.z < 0f)
				{
					if (body.velocity.z > 0f - maxMoveSpeed && velocity.z <= 0f - maxMoveSpeed)
					{
						velocity.z = 0f - maxMoveSpeed;
					}
					else if (body.velocity.z < 0f - maxMoveSpeed)
					{
						velocity.z = body.velocity.z;
					}
				}
				else if (body.velocity.z < maxMoveSpeed && velocity.z >= maxMoveSpeed)
				{
					velocity.z = maxMoveSpeed;
				}
				else if (body.velocity.z > maxMoveSpeed)
				{
					velocity.z = body.velocity.z;
				}
				body.velocity = velocity;
			}
			if (vector.magnitude <= movementDeadzone && isGrounded)
			{
				body.velocity *= 1f - Mathf.Clamp01(groundedDrag * (Time.fixedDeltaTime - lastUpdateTime));
			}
			if (IsClimbing() || pushAxis.y > 0f)
			{
				body.useGravity = false;
			}
			if (body.useGravity)
			{
				body.velocity = new Vector3(body.velocity.x, y, body.velocity.z);
			}
			SyncBodyHead();
			ignoreIterpolationFrame = false;
			lastUpdateTime = Time.fixedDeltaTime;
		}

		private void SyncBodyHead()
		{
			float num = 50f * Time.fixedDeltaTime;
			float num2 = ((base.transform.lossyScale.x > base.transform.lossyScale.z) ? base.transform.lossyScale.x : base.transform.lossyScale.z);
			if (!((headCamera.transform.position - base.transform.position).magnitude > 0.1f * num))
			{
				return;
			}
			Vector3 vector = headCamera.transform.position - base.transform.position;
			vector.y = 0f;
			Debug.DrawLine(base.transform.position, base.transform.position + vector.normalized * 0.03f, Color.yellow);
			if (!Physics.CheckCapsule(vector * 0.1f * num + num2 * base.transform.position + Vector3.up * num2 * bodyCapsule.radius, vector * 0.1f * num + base.transform.position - num2 * Vector3.up * bodyCapsule.radius + num2 * Vector3.up * bodyCapsule.height, num2 * bodyCapsule.radius, handPlayerMask, QueryTriggerInteraction.Ignore))
			{
				offset = vector * 0.1f * num;
				base.transform.position += offset;
				targetTrackedPos -= offset;
				return;
			}
			for (int i = -80; i <= 80; i += 40)
			{
				Vector3 vector2 = Quaternion.Euler(0f, i, 0f) * vector;
				Debug.DrawLine(base.transform.position, base.transform.position + vector2.normalized * 0.1f, Color.yellow);
				if (!Physics.CheckCapsule(vector2 * 0.1f * num + num2 * base.transform.position + Vector3.up * num2 * bodyCapsule.radius, vector2 * 0.1f * num + base.transform.position - num2 * Vector3.up * bodyCapsule.radius + num2 * Vector3.up * bodyCapsule.height, num2 * bodyCapsule.radius, handPlayerMask, QueryTriggerInteraction.Ignore))
				{
					offset = vector2 * 0.1f * num;
					base.transform.position += offset;
					targetTrackedPos -= offset;
					break;
				}
			}
		}

		protected virtual bool CanInputMove()
		{
			if (!allowClimbingMovement)
			{
				return !IsClimbing();
			}
			return true;
		}

		protected virtual void InterpolateMovement()
		{
			float num = Time.deltaTime - lastUpdateTime;
			Vector3 position = handRight.transform.position;
			Vector3 position2 = handLeft.transform.position;
			if (body.drag > 0f)
			{
				body.velocity *= 1f - Mathf.Clamp01(body.drag * num);
			}
			Vector3 vector = AlterDirection(moveDirection);
			if (vector.magnitude <= movementDeadzone && isGrounded)
			{
				body.velocity *= 1f - Mathf.Clamp01(groundedDrag * num);
			}
			float y = body.velocity.y;
			Vector3 vector2 = Vector3.MoveTowards(body.velocity, vector * maxMoveSpeed, moveAcceleration * Time.fixedDeltaTime);
			if (vector.x < 0f)
			{
				if (body.velocity.x > 0f - maxMoveSpeed && vector2.x <= 0f - maxMoveSpeed)
				{
					vector2.x = 0f - maxMoveSpeed;
				}
				else if (body.velocity.x < 0f - maxMoveSpeed)
				{
					vector2.x = body.velocity.x;
				}
			}
			else if (body.velocity.x < maxMoveSpeed && vector2.x >= maxMoveSpeed)
			{
				vector2.x = maxMoveSpeed;
			}
			else if (body.velocity.x > maxMoveSpeed)
			{
				vector2.x = body.velocity.x;
			}
			if (vector.z < 0f)
			{
				if (body.velocity.z > 0f - maxMoveSpeed && vector2.z <= 0f - maxMoveSpeed)
				{
					vector2.z = 0f - maxMoveSpeed;
				}
				else if (body.velocity.z < 0f - maxMoveSpeed)
				{
					vector2.z = body.velocity.z;
				}
			}
			else if (body.velocity.z < maxMoveSpeed && vector2.z >= maxMoveSpeed)
			{
				vector2.z = maxMoveSpeed;
			}
			else if (body.velocity.z > maxMoveSpeed)
			{
				vector2.z = body.velocity.z;
			}
			body.position = Vector3.MoveTowards(body.position, body.position + vector2, vector2.magnitude * num);
			if (body.useGravity)
			{
				body.velocity = new Vector3(body.velocity.x, y, body.velocity.z);
			}
			base.transform.position = body.position;
			if (!ignoreIterpolationFrame)
			{
				targetTrackedPos += base.transform.position - lastUpdatePosition;
				Vector3 position3 = new Vector3(targetTrackedPos.x, trackingContainer.position.y, targetTrackedPos.z);
				trackingContainer.position = position3;
				if (isGrounded)
				{
					trackingContainer.position = Vector3.MoveTowards(trackingContainer.position, targetTrackedPos + Vector3.up * heightOffset, (Mathf.Abs(trackingContainer.position.y - targetTrackedPos.y) + 0.1f) * Time.deltaTime * heightSmoothSpeed);
				}
				else
				{
					trackingContainer.position = targetTrackedPos + Vector3.up * heightOffset;
				}
				Vector3 target = base.transform.position - headCamera.transform.position;
				target.y = 0f;
				targetPosOffset = Vector3.MoveTowards(targetPosOffset, target, body.velocity.magnitude * Time.deltaTime * 2f);
				trackingContainer.position += targetPosOffset;
				if (headPhysicsFollower != null && isGrounded && Vector3.Distance(headCamera.transform.position, headPhysicsFollower.transform.position) > headPhysicsFollower.headCollider.radius / 1.5f)
				{
					Vector3 vector3 = headPhysicsFollower.transform.position + (headCamera.transform.position - headPhysicsFollower.transform.position).normalized * headPhysicsFollower.headCollider.radius / 1.5f;
					Vector3 vector4 = headCamera.transform.position - vector3;
					trackingContainer.position -= vector4;
				}
				Vector3 vector5 = handRight.transform.position - position;
				RaycastHit hitInfo;
				if (pushRight.Count > 0)
				{
					handRight.transform.position -= vector5;
				}
				else if (handRight.body.SweepTest(vector5, out hitInfo, vector5.magnitude) && (handRight.holdingObj == null || (hitInfo.rigidbody != handRight.holdingObj.body && !handRight.holdingObj.jointedBodies.Contains(hitInfo.rigidbody))) && (handLeft.holdingObj == null || (hitInfo.rigidbody != handLeft.holdingObj.body && !handLeft.holdingObj.jointedBodies.Contains(hitInfo.rigidbody))))
				{
					handRight.transform.position -= vector5;
				}
				vector5 = handLeft.transform.position - position2;
				RaycastHit hitInfo2;
				if (pushLeft.Count > 0)
				{
					handLeft.transform.position -= vector5;
				}
				else if (handLeft.body.SweepTest(vector5, out hitInfo2, vector5.magnitude) && (handRight.holdingObj == null || (hitInfo2.rigidbody != handRight.holdingObj.body && !handRight.holdingObj.jointedBodies.Contains(hitInfo2.rigidbody))) && (handLeft.holdingObj == null || (hitInfo2.rigidbody != handLeft.holdingObj.body && !handLeft.holdingObj.jointedBodies.Contains(hitInfo2.rigidbody))))
				{
					handLeft.transform.position -= vector5;
				}
			}
			lastUpdatePosition = base.transform.position;
			lastUpdateTime = Time.deltaTime;
		}

		protected virtual void UpdateTurn(float deltaTime)
		{
			if (snapTurning)
			{
				if (Mathf.Abs(turningAxis) > turnDeadzone && axisReset)
				{
					float angle = ((turningAxis > turnDeadzone) ? snapTurnAngle : (0f - snapTurnAngle));
					Vector3 vector = base.transform.position - headCamera.transform.position;
					vector.y = 0f;
					trackingContainer.position += vector;
					if (headPhysicsFollower != null)
					{
						headPhysicsFollower.transform.position += vector;
						headPhysicsFollower.body.position = headPhysicsFollower.transform.position;
					}
					lastUpdatePosition = new Vector3(base.transform.position.x, lastUpdatePosition.y, base.transform.position.z);
					trackingContainer.RotateAround(base.transform.position, Vector3.up, angle);
					targetPosOffset = Vector3.zero;
					targetTrackedPos = new Vector3(trackingContainer.position.x, targetTrackedPos.y, trackingContainer.position.z);
					handRight.body.position = handRight.transform.position;
					handLeft.body.position = handLeft.transform.position;
					handRight.SetHandLocation(handRight.transform.position);
					handLeft.SetHandLocation(handLeft.transform.position);
					axisReset = false;
				}
			}
			else if (Mathf.Abs(turningAxis) > turnDeadzone)
			{
				Vector3 vector2 = base.transform.position - headCamera.transform.position;
				vector2.y = 0f;
				trackingContainer.position += vector2;
				if (headPhysicsFollower != null)
				{
					headPhysicsFollower.transform.position += vector2;
					headPhysicsFollower.body.position = headPhysicsFollower.transform.position;
				}
				lastUpdatePosition = new Vector3(base.transform.position.x, lastUpdatePosition.y, base.transform.position.z);
				trackingContainer.RotateAround(base.transform.position, Vector3.up, smoothTurnSpeed * Mathf.MoveTowards(turningAxis, 0f, turnDeadzone) * deltaTime);
				targetPosOffset = Vector3.zero;
				targetTrackedPos = new Vector3(trackingContainer.position.x, targetTrackedPos.y, trackingContainer.position.z);
				axisReset = false;
			}
			if (Mathf.Abs(turningAxis) < turnResetzone)
			{
				axisReset = true;
			}
		}

		protected virtual void Ground()
		{
			isGrounded = false;
			newClosestHit = default(RaycastHit);
			if (!tempDisableGrounding && useGrounding && !IsClimbing() && !(pushAxis.y > 0f))
			{
				highestPoint = -1f;
				CheckGroundRadius(2, 0f);
				CheckGroundRadius(21, 1f);
				CheckGroundRadius(10, 0.75f);
				CheckGroundRadius(7, 0.5f);
				CheckGroundRadius(5, 0.25f);
				if (isGrounded)
				{
					body.velocity = new Vector3(body.velocity.x, 0f, body.velocity.z);
					body.position += Vector3.up * (highestPoint - groundedOffset / 2f);
					base.transform.position = body.position;
				}
				body.useGravity = !isGrounded;
			}
			void CheckGroundRadius(int groundRayCount, float multi)
			{
				float radius = bodyCapsule.radius;
				float num = ((base.transform.lossyScale.x > base.transform.lossyScale.z) ? base.transform.lossyScale.x : base.transform.lossyScale.z);
				for (int i = 0; i < groundRayCount; i++)
				{
					float num2 = maxStepHeight;
					num2 *= ((climbAxis.y > 0f) ? climbUpStepHeightMultiplier : 1f);
					num2 *= ((pushAxis.y > 0f) ? pushUpStepHeightMultiplier : 1f);
					Vector3 position = base.transform.position;
					position.x += Mathf.Cos((float)i * MathF.PI / (float)(groundRayCount / 2)) * (num * radius + 0.15f) * multi;
					position.z += Mathf.Sin((float)i * MathF.PI / (float)(groundRayCount / 2)) * (num * radius + 0.15f) * multi;
					position.y += num2;
					Debug.DrawRay(position, -Vector3.up * (num2 + groundedOffset), Color.red, Time.fixedDeltaTime);
					if (Physics.Raycast(position, -Vector3.up, out var hitInfo, num2 + groundedOffset, groundLayerMask, QueryTriggerInteraction.Ignore))
					{
						float num3 = Vector3.Angle(hitInfo.normal, Vector3.up);
						float num4 = Vector3.Distance(hitInfo.point, position - Vector3.up * (num2 + groundedOffset));
						if (num3 < maxStepAngle && num4 > highestPoint)
						{
							isGrounded = true;
							highestPoint = num4;
							newClosestHit = hitInfo;
						}
					}
				}
			}
		}

		public bool IsGrounded()
		{
			return isGrounded;
		}

		public void ToggleFlying()
		{
			useGrounding = !useGrounding;
			body.useGravity = useGrounding;
		}

		protected virtual void UpdatePlayerHeight()
		{
			if (crouching != lastCrouching)
			{
				if (lastCrouching)
				{
					heightOffset += lastCrouchingHeight;
				}
				if (!lastCrouching)
				{
					heightOffset -= crouchHeight;
				}
				lastCrouching = crouching;
				lastCrouchingHeight = crouchHeight;
			}
			if (autoAdjustColliderHeight)
			{
				playerHeight = Mathf.Clamp(headCamera.transform.position.y - base.transform.position.y, minMaxHeight.x, minMaxHeight.y);
				bodyCapsule.height = playerHeight;
				float y = ((playerHeight / 2f > bodyCapsule.radius) ? (playerHeight / 2f) : bodyCapsule.radius);
				bodyCapsule.center = new Vector3(0f, y, 0f);
			}
		}

		protected void UpdatePlatform(bool isFixedUpdate)
		{
			if ((!ignoreIterpolationFrame || isFixedUpdate) && isGrounded && newClosestHit.transform != null && (int)platformingLayerMask == ((int)platformingLayerMask | (1 << newClosestHit.collider.gameObject.layer)))
			{
				if (newClosestHit.transform != closestHit.transform)
				{
					closestHit = newClosestHit;
					lastPlatformPosition = closestHit.transform.position;
					lastPlatformRotation = closestHit.transform.rotation;
				}
				else if (newClosestHit.transform == closestHit.transform && (closestHit.transform.position != lastPlatformPosition || closestHit.transform.rotation != lastPlatformRotation))
				{
					closestHit = newClosestHit;
					base.transform.position += closestHit.transform.position - lastPlatformPosition;
					Quaternion quaternion = closestHit.transform.rotation * Quaternion.Inverse(lastPlatformRotation);
					base.transform.RotateAround(closestHit.transform.position, Vector3.up, quaternion.eulerAngles.y);
					body.position = base.transform.position;
					body.rotation = base.transform.rotation;
					quaternion.eulerAngles = new Vector3(0f, quaternion.eulerAngles.y, 0f);
					trackingContainer.rotation *= quaternion;
					lastPlatformPosition = closestHit.transform.position;
					lastPlatformRotation = closestHit.transform.rotation;
				}
			}
		}

		public void Jump(float jumpPower = 1f)
		{
			if (isGrounded)
			{
				DisableGrounding(0.1f);
				body.useGravity = true;
				body.AddForce(Vector3.up * jumpPower, ForceMode.VelocityChange);
			}
		}

		public void DisableGrounding(float seconds)
		{
			if (disableGroundingRoutine != null)
			{
				StopCoroutine(disableGroundingRoutine);
			}
			disableGroundingRoutine = StartCoroutine(DisableGroundingSecondsRoutine(seconds));
		}

		private IEnumerator DisableGroundingSecondsRoutine(float seconds)
		{
			tempDisableGrounding = true;
			isGrounded = false;
			yield return new WaitForSeconds(seconds);
			tempDisableGrounding = false;
		}

		public void AddVelocity(Vector3 force, ForceMode mode = ForceMode.Acceleration)
		{
			body.AddForce(force, mode);
		}

		protected virtual void StartPush(Hand hand, GameObject other)
		{
			if (!allowBodyPushing || IsClimbing() || !other.CanGetComponent<Pushable>(out var component) || !component.enabled)
			{
				return;
			}
			if (hand.left)
			{
				if (!pushLeft.ContainsKey(component))
				{
					pushLeft.Add(component, hand);
					pushLeftCount.Add(component, 1);
				}
				else
				{
					pushLeftCount[component]++;
				}
			}
			if (!hand.left && !pushRight.ContainsKey(component))
			{
				if (!pushRight.ContainsKey(component))
				{
					pushRight.Add(component, hand);
					pushRightCount.Add(component, 1);
				}
				else
				{
					pushRightCount[component]++;
				}
			}
		}

		protected virtual void StopPush(Hand hand, GameObject other)
		{
			if (allowBodyPushing && other.CanGetComponent<Pushable>(out var component))
			{
				if (hand.left && pushLeft.ContainsKey(component) && --pushLeftCount[component] == 0)
				{
					pushLeft.Remove(component);
					pushLeftCount.Remove(component);
				}
				if (!hand.left && pushRight.ContainsKey(component) && --pushRightCount[component] == 0)
				{
					pushRight.Remove(component);
					pushRightCount.Remove(component);
				}
			}
		}

		protected virtual void StartGrabPush(Hand hand, Grabbable grab)
		{
			if (allowBodyPushing && grab.CanGetComponent<Pushable>(out var component) && component.enabled)
			{
				if (hand.left && !pushLeft.ContainsKey(component))
				{
					pushLeft.Add(component, hand);
					pushLeftCount.Add(component, 1);
				}
				if (!hand.left && !pushRight.ContainsKey(component))
				{
					pushRight.Add(component, hand);
					pushRightCount.Add(component, 1);
				}
			}
		}

		protected virtual void EndGrabPush(Hand hand, Grabbable grab)
		{
			if (grab != null && grab.CanGetComponent<Pushable>(out var component))
			{
				if (hand.left && pushLeft.ContainsKey(component))
				{
					pushLeft.Remove(component);
					pushLeftCount.Remove(component);
				}
				else if (!hand.left && pushRight.ContainsKey(component))
				{
					pushRight.Remove(component);
					pushRightCount.Remove(component);
				}
			}
		}

		protected virtual void ApplyPushingForce()
		{
			pushAxis = Vector3.zero;
			if (!allowBodyPushing)
			{
				return;
			}
			RaycastHit[] array = Physics.RaycastAll(handRight.transform.position, Vector3.down, 0.1f, ~handRight.handLayers);
			RaycastHit[] array2 = Physics.RaycastAll(handLeft.transform.position, Vector3.down, 0.1f, ~handLeft.handLayers);
			List<GameObject> list = new List<GameObject>();
			RaycastHit[] array3 = array;
			foreach (RaycastHit raycastHit in array3)
			{
				list.Add(raycastHit.transform.gameObject);
			}
			array3 = array2;
			foreach (RaycastHit raycastHit2 in array3)
			{
				list.Add(raycastHit2.transform.gameObject);
			}
			foreach (KeyValuePair<Pushable, Hand> item in pushRight)
			{
				if (item.Key.enabled && !item.Value.IsGrabbing())
				{
					Vector3 a = Vector3.zero;
					if (Vector3.Distance(item.Value.body.position, item.Value.moveTo.position) > 0f)
					{
						a = Vector3.Scale(item.Value.body.position - item.Value.moveTo.position, item.Key.strengthScale);
					}
					a = Vector3.Scale(a, pushingStrength);
					if (!list.Contains(item.Key.transform.gameObject))
					{
						a.y = 0f;
					}
					pushAxis += a / 2f;
				}
			}
			foreach (KeyValuePair<Pushable, Hand> item2 in pushLeft)
			{
				if (item2.Key.enabled && !item2.Value.IsGrabbing())
				{
					Vector3 a2 = Vector3.zero;
					if (Vector3.Distance(item2.Value.body.position, item2.Value.moveTo.position) > 0f)
					{
						a2 = Vector3.Scale(item2.Value.body.position - item2.Value.moveTo.position, item2.Key.strengthScale);
					}
					a2 = Vector3.Scale(a2, pushingStrength);
					if (!list.Contains(item2.Key.transform.gameObject))
					{
						a2.y = 0f;
					}
					pushAxis += a2 / 2f;
				}
			}
		}

		public bool IsPushing()
		{
			foreach (KeyValuePair<Pushable, Hand> item in pushRight)
			{
				if (item.Key.enabled)
				{
					return true;
				}
			}
			foreach (KeyValuePair<Pushable, Hand> item2 in pushLeft)
			{
				if (item2.Key.enabled)
				{
					return true;
				}
			}
			return false;
		}

		protected virtual void StartClimb(Hand hand, Grabbable grab)
		{
			if (allowClimbing && !climbing.ContainsKey(hand) && grab != null && grab.CanGetComponent<Climbable>(out var component) && component.enabled)
			{
				if (climbing.Count == 0)
				{
					pushRight.Clear();
					pushRightCount.Clear();
					pushLeft.Clear();
					pushLeftCount.Clear();
				}
				if (climbing.Count == 0)
				{
					body.velocity /= 4f;
				}
				climbing.Add(hand, component);
			}
		}

		protected virtual void EndClimb(Hand hand, Grabbable grab)
		{
			if (!allowClimbing)
			{
				return;
			}
			if (climbing.ContainsKey(hand))
			{
				climbing.Remove(hand);
			}
			foreach (KeyValuePair<Hand, Climbable> item in climbing)
			{
				item.Key.ResetGrabOffset();
			}
		}

		protected virtual void ApplyClimbingForce()
		{
			climbAxis = Vector3.zero;
			if (!allowClimbing || climbing.Count <= 0)
			{
				return;
			}
			foreach (KeyValuePair<Hand, Climbable> item in climbing)
			{
				if (item.Value.enabled)
				{
					Vector3 a = Vector3.Scale(item.Key.body.position - item.Key.moveTo.position, item.Value.axis);
					a = Vector3.Scale(a, climbingStrength);
					climbAxis += a / climbing.Count;
				}
			}
		}

		public bool IsClimbing()
		{
			foreach (KeyValuePair<Hand, Climbable> item in climbing)
			{
				if (item.Value.enabled)
				{
					return true;
				}
			}
			return false;
		}

		public virtual void SetPosition(Vector3 position)
		{
			SetPosition(position, headCamera.transform.rotation);
		}

		public virtual void SetPosition(Vector3 position, Quaternion rotation)
		{
			Vector3 vector = position - base.transform.position;
			base.transform.position += vector;
			Vector3 vector2 = base.transform.position - headCamera.transform.position;
			vector2.y = vector.y;
			trackingContainer.position += vector2;
			lastUpdatePosition = base.transform.position;
			targetTrackedPos = new Vector3(trackingContainer.position.x, targetTrackedPos.y + vector.y, trackingContainer.position.z);
			targetPosOffset = Vector3.zero;
			body.position = base.transform.position;
			if (headPhysicsFollower != null)
			{
				headPhysicsFollower.transform.position += vector2;
				headPhysicsFollower.body.position = headPhysicsFollower.transform.position;
			}
			handRight.body.position = handRight.transform.position;
			handLeft.body.position = handLeft.transform.position;
			handRight.SetHandLocation(handRight.transform.position);
			handLeft.SetHandLocation(handLeft.transform.position);
			Quaternion quaternion = rotation * Quaternion.Inverse(headCamera.transform.rotation);
			trackingContainer.RotateAround(headCamera.transform.position, Vector3.up, quaternion.eulerAngles.y);
		}

		public virtual void SetRotation(Quaternion rotation)
		{
			Vector3 vector = base.transform.position - headCamera.transform.position;
			vector.y = 0f;
			trackingContainer.position += vector;
			if (headPhysicsFollower != null)
			{
				headPhysicsFollower.transform.position += vector;
				headPhysicsFollower.body.position = headPhysicsFollower.transform.position;
			}
			lastUpdatePosition = base.transform.position;
			Quaternion quaternion = rotation * Quaternion.Inverse(headCamera.transform.rotation);
			trackingContainer.RotateAround(headCamera.transform.position, Vector3.up, quaternion.eulerAngles.y);
			targetPosOffset = Vector3.zero;
			targetTrackedPos = new Vector3(trackingContainer.position.x, targetTrackedPos.y, trackingContainer.position.z);
		}

		public virtual void AddRotation(Quaternion addRotation)
		{
			Vector3 vector = base.transform.position - headCamera.transform.position;
			vector.y = 0f;
			trackingContainer.position += vector;
			if (headPhysicsFollower != null)
			{
				headPhysicsFollower.transform.position += vector;
				headPhysicsFollower.body.position = headPhysicsFollower.transform.position;
			}
			lastUpdatePosition = base.transform.position;
			trackingContainer.RotateAround(headCamera.transform.position, Vector3.up, addRotation.eulerAngles.y);
			targetPosOffset = Vector3.zero;
			targetTrackedPos = new Vector3(trackingContainer.position.x, targetTrackedPos.y, trackingContainer.position.z);
		}

		public virtual void Recenter()
		{
			Vector3 vector = base.transform.position - headCamera.transform.position;
			vector.y = 0f;
			trackingContainer.position += vector;
			if (headPhysicsFollower != null)
			{
				headPhysicsFollower.transform.position += vector;
				headPhysicsFollower.body.position = headPhysicsFollower.transform.position;
			}
			lastUpdatePosition = base.transform.position;
			targetPosOffset = Vector3.zero;
			targetTrackedPos = new Vector3(trackingContainer.position.x, targetTrackedPos.y, trackingContainer.position.z);
		}

		private Vector3 AlterDirection(Vector3 moveAxis)
		{
			if (useGrounding)
			{
				return Quaternion.AngleAxis(forwardFollow.eulerAngles.y, Vector3.up) * new Vector3(moveAxis.x, moveAxis.y, moveAxis.z);
			}
			return forwardFollow.rotation * new Vector3(moveAxis.x, moveAxis.y, moveAxis.z);
		}
	}
}
