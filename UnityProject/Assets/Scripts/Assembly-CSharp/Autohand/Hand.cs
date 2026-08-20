using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace Autohand
{
	[HelpURL("https://earnestrobot.notion.site/Hand-967e36c2ab2945b2b0f75cea84624b2f")]
	[DefaultExecutionOrder(-10)]
	public class Hand : HandBase
	{
		[AutoToggleHeader("Enable Highlight", 0, 0, tooltip = "Raycasting for grabbables to highlight is expensive, you can disable it here if you aren't using it")]
		public bool usingHighlight = true;

		[EnableIf("usingHighlight")]
		[Tooltip("The layers to highlight and use look assist on --- Nothing will default on start")]
		public LayerMask highlightLayers;

		[EnableIf("usingHighlight")]
		[Tooltip("Leave empty for none - used as a default option for all grabbables with empty highlight material")]
		public Material defaultHighlight;

		[AutoToggleHeader("Show Advanced", 0, 0)]
		public bool showAdvanced;

		[ShowIf("showAdvanced")]
		[Tooltip("Whether the hand should go to the object and come back on grab, or the object to float to the hand on grab. Will default to HandToGrabbable for objects that have \"parentOnGrab\" disabled")]
		public GrabType grabType;

		[ShowIf("showAdvanced")]
		[Tooltip("Makes grab smoother; also based on range and reach distance - a very near grab is minGrabTime and a max distance grab is maxGrabTime")]
		[Min(0f)]
		public float minGrabTime = 0.1f;

		[ShowIf("showAdvanced")]
		[Tooltip("Makes grab smoother; also based on range and reach distance - a very near grab is minGrabTime and a max distance grab is maxGrabTime")]
		[Min(0f)]
		public float maxGrabTime = 0.25f;

		[ShowIf("showAdvanced")]
		[Tooltip("The animation curve based on the grab time 0-1")]
		[Min(0f)]
		public AnimationCurve grabCurve;

		[ShowIf("showAdvanced")]
		[Tooltip("Speed at which the gentle grab returns the grabbable")]
		[Min(0f)]
		[FormerlySerializedAs("smoothReturnSpeed")]
		public float gentleGrabSpeed = 1f;

		[ShowIf("showAdvanced")]
		[Tooltip("This is used in conjunction with custom poses. For a custom pose to work it must has the same PoseIndex as the hand. Used for when your game has multiple hands")]
		public int poseIndex;

		[AutoLine(0, 0)]
		public bool ignoreMe1;

		public static string[] grabbableLayers = new string[2] { "Grabbable", "Grabbing" };

		public static string grabbableLayerNameDefault = "Grabbable";

		public static string grabbingLayerName = "Grabbing";

		public static string rightHandLayerName = "Hand";

		public static string leftHandLayerName = "Hand";

		private List<HandTriggerAreaEvents> triggerEventAreas = new List<HandTriggerAreaEvents>();

		private Coroutine tryGrab;

		private Coroutine highlightRoutine;

		private float startGrabDist;

		private HandPoseData openHandPose;

		private Grabbable lastHoldingObj;

		private Coroutine _grabRoutine;

		public Hand copyFromHand;

		private Vector3 startHandGrabPosition;

		private Coroutine grabRoutine
		{
			get
			{
				return _grabRoutine;
			}
			set
			{
				if (value != null && _grabRoutine != null)
				{
					StopCoroutine(_grabRoutine);
					if (base.holdingObj != null)
					{
						base.holdingObj.body.velocity = Vector3.zero;
						base.holdingObj.body.angularVelocity = Vector3.zero;
						base.holdingObj.beingGrabbed = false;
					}
					BreakGrabConnection();
					grabbing = false;
				}
				_grabRoutine = value;
			}
		}

		public event HandGrabEvent OnTriggerGrab;

		public event HandGrabEvent OnBeforeGrabbed;

		public event HandGrabEvent OnGrabbed;

		public event HandGrabEvent OnTriggerRelease;

		public event HandGrabEvent OnBeforeReleased;

		public event HandGrabEvent OnReleased;

		public event HandGrabEvent OnSqueezed;

		public event HandGrabEvent OnUnsqueezed;

		public event HandGrabEvent OnHighlight;

		public event HandGrabEvent OnStopHighlight;

		public event HandGrabEvent OnForcedRelease;

		public event HandGrabEvent OnGrabJointBreak;

		public event HandGrabEvent OnHeldConnectionBreak;

		public event HandGameObjectEvent OnHandCollisionStart;

		public event HandGameObjectEvent OnHandCollisionStop;

		public event HandGameObjectEvent OnHandTriggerStart;

		public event HandGameObjectEvent OnHandTriggerStop;

		protected override void Awake()
		{
			if ((int)highlightLayers == 0)
			{
				highlightLayers = LayerMask.GetMask(grabbableLayerNameDefault);
			}
			handLayers = LayerMask.GetMask(rightHandLayerName, leftHandLayerName);
			base.Awake();
			if (enableMovement)
			{
				base.body.drag = 10f;
				base.body.angularDrag = 38f;
				base.body.useGravity = false;
			}
			SetLayer();
			base.Awake();
		}

		private void Start()
		{
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			highlightRoutine = StartCoroutine(HighlightUpdate(Time.fixedUnscaledDeltaTime * 4f));
			base.collisionTracker.OnCollisionFirstEnter += OnCollisionFirstEnter;
			base.collisionTracker.OnCollisionLastExit += OnCollisionLastExit;
			base.collisionTracker.OnTriggerFirstEnter += OnTriggerFirstEnter;
			base.collisionTracker.OnTriggeLastExit += OnTriggerLastExit;
			base.collisionTracker.OnCollisionFirstEnter += delegate(GameObject collision)
			{
				this.OnHandCollisionStart?.Invoke(this, collision);
			};
			base.collisionTracker.OnCollisionLastExit += delegate(GameObject collision)
			{
				this.OnHandCollisionStop?.Invoke(this, collision);
			};
			base.collisionTracker.OnTriggerFirstEnter += delegate(GameObject collision)
			{
				this.OnHandTriggerStart?.Invoke(this, collision);
			};
			base.collisionTracker.OnTriggeLastExit += delegate(GameObject collision)
			{
				this.OnHandTriggerStop?.Invoke(this, collision);
			};
		}

		protected override void OnDisable()
		{
			foreach (HandTriggerAreaEvents triggerEventArea in triggerEventAreas)
			{
				triggerEventArea.Exit(this);
			}
			if (tryGrab != null)
			{
				StopCoroutine(tryGrab);
			}
			if (highlightRoutine != null)
			{
				StopCoroutine(highlightRoutine);
			}
			base.OnDisable();
			base.collisionTracker.OnCollisionFirstEnter -= OnCollisionFirstEnter;
			base.collisionTracker.OnCollisionLastExit -= OnCollisionLastExit;
			base.collisionTracker.OnTriggerFirstEnter -= OnTriggerFirstEnter;
			base.collisionTracker.OnTriggeLastExit -= OnTriggerLastExit;
			base.collisionTracker.OnCollisionFirstEnter -= delegate(GameObject collision)
			{
				this.OnHandCollisionStart?.Invoke(this, collision);
			};
			base.collisionTracker.OnCollisionLastExit -= delegate(GameObject collision)
			{
				this.OnHandCollisionStop?.Invoke(this, collision);
			};
			base.collisionTracker.OnTriggerFirstEnter -= delegate(GameObject collision)
			{
				this.OnHandTriggerStart?.Invoke(this, collision);
			};
			base.collisionTracker.OnTriggeLastExit -= delegate(GameObject collision)
			{
				this.OnHandTriggerStop?.Invoke(this, collision);
			};
		}

		protected override void Update()
		{
			if (enableMovement)
			{
				if ((bool)base.holdingObj && !base.holdingObj.maintainGrabOffset && !IsGrabbing())
				{
					float num = Vector3.Distance(follow.position, lastFrameFollowPos);
					float num2 = Quaternion.Angle(follow.rotation, lastFrameFollowRot);
					base.grabPositionOffset = Vector3.MoveTowards(base.grabPositionOffset, Vector3.zero, num * gentleGrabSpeed * Time.deltaTime * 60f);
					base.grabRotationOffset = Quaternion.RotateTowards(base.grabRotationOffset, Quaternion.identity, num2 * gentleGrabSpeed * Time.deltaTime * 60f);
					if (!base.holdingObj.useGentleGrab)
					{
						base.grabPositionOffset = Vector3.MoveTowards(base.grabPositionOffset, Vector3.zero, Time.deltaTime / GetGrabTime());
						base.grabRotationOffset = Quaternion.RotateTowards(base.grabRotationOffset, Quaternion.identity, 90f * Time.deltaTime / GetGrabTime());
					}
				}
				lastFrameFollowPos = follow.position;
				lastFrameFollowRot = follow.rotation;
			}
			base.Update();
		}

		private float GetGrabTime()
		{
			float num = Mathf.Clamp01(startGrabDist / reachDistance);
			float num2 = ((base.holdingObj.body != null) ? (base.holdingObj.body.velocity.magnitude * Time.fixedDeltaTime * num * 2f) : 0f);
			float num3 = base.body.velocity.magnitude * Time.fixedDeltaTime * num * 2f;
			return Mathf.Clamp(minGrabTime + (maxGrabTime - minGrabTime) * num - num2 - num3, 0f, maxGrabTime);
		}

		public virtual void Grab()
		{
			GrabType grabType = this.grabType;
			Grab(grabType);
		}

		public virtual void Grab(GrabType grabType)
		{
			this.OnTriggerGrab?.Invoke(this, null);
			foreach (HandTriggerAreaEvents triggerEventArea in triggerEventAreas)
			{
				triggerEventArea.Grab(this);
			}
			GrabLock component;
			if (usingHighlight && !grabbing && base.holdingObj == null && base.lookingAtObj != null)
			{
				GrabType grabType2 = this.grabType;
				if (base.lookingAtObj.grabType != HandGrabType.Default)
				{
					grabType2 = ((base.lookingAtObj.grabType == HandGrabType.GrabbableToHand) ? GrabType.GrabbableToHand : GrabType.HandToGrabbable);
				}
				grabRoutine = StartCoroutine(GrabObject(GetHighlightHit(), base.lookingAtObj, grabType2));
			}
			else if (!grabbing && base.holdingObj == null)
			{
				if (HandClosestHit(out var closestHit, out var grabbable, reachDistance, ~handLayers) != Vector3.zero && grabbable != null)
				{
					GrabType grabType3 = this.grabType;
					if (grabbable.grabType != HandGrabType.Default)
					{
						grabType3 = ((grabbable.grabType == HandGrabType.GrabbableToHand) ? GrabType.GrabbableToHand : GrabType.HandToGrabbable);
					}
					if (grabbable != null)
					{
						grabRoutine = StartCoroutine(GrabObject(closestHit, grabbable, grabType3));
					}
				}
			}
			else if (base.holdingObj != null && base.holdingObj.CanGetComponent<GrabLock>(out component))
			{
				component.OnGrabPressed?.Invoke();
			}
		}

		public virtual void Grab(RaycastHit hit, Grabbable grab, GrabType grabType = GrabType.InstantGrab)
		{
			bool flag = !grab.body.isKinematic && grab.body.constraints == RigidbodyConstraints.None;
			if (!grabbing && base.holdingObj == null && CanGrab(grab) && flag)
			{
				grabRoutine = StartCoroutine(GrabObject(hit, grab, grabType));
			}
		}

		public virtual void TryGrab(Grabbable grab)
		{
			if (grabbing || !(base.holdingObj == null) || !CanGrab(grab))
			{
				return;
			}
			grab.body.position = palmTransform.position + palmTransform.forward * reachDistance;
			grab.body.transform.position = grab.body.position;
			int layer = grab.gameObject.layer;
			int num = LayerMask.NameToLayer(grabbingLayerName);
			Vector3 worldCenterOfMass = grab.body.worldCenterOfMass;
			SetLayerRecursive(grab.transform, num);
			RaycastHit hit = default(RaycastHit);
			bool flag = false;
			for (int i = 0; i < 3; i++)
			{
				if (grabbing || !(base.holdingObj == null))
				{
					continue;
				}
				Ray ray = new Ray(palmTransform.position, worldCenterOfMass - palmTransform.position);
				if (Physics.SphereCast(ray.origin - ray.direction * sphereCastRadius * 10f, sphereCastRadius * 3f, ray.direction.normalized, out var hitInfo, Vector3.Distance(worldCenterOfMass, palmTransform.position) * 2f + sphereCastRadius * 10f, 1 << num, queryTriggerInteraction))
				{
					Vector3 vector = worldCenterOfMass - hitInfo.point;
					grab.body.position = palmTransform.position - ray.direction.normalized * reachDistance * 0.5f + vector;
					grab.body.transform.position = grab.body.position;
					hit = hitInfo;
					flag = true;
					if (HandClosestHit(out var closestHit, out var grabbable, reachDistance * 2f, 1 << num) != Vector3.zero && grabbable != null)
					{
						SetLayerRecursive(grab.transform, layer);
						grabbable.body.velocity = Vector3.zero;
						grabbable.body.angularVelocity = Vector3.zero;
						grabRoutine = StartCoroutine(GrabObject(closestHit, grabbable, GrabType.InstantGrab));
					}
				}
			}
			if (!grabbing && base.holdingObj == null && flag)
			{
				Grab(hit, grab);
			}
		}

		public virtual void Release()
		{
			this.OnTriggerRelease?.Invoke(this, null);
			foreach (HandTriggerAreaEvents triggerEventArea in triggerEventAreas)
			{
				triggerEventArea.Release(this);
			}
			if (!base.holdingObj || base.holdingObj.wasForceReleased || !base.holdingObj.CanGetComponent<GrabLock>(out var _))
			{
				if (base.holdingObj != null)
				{
					this.OnBeforeReleased?.Invoke(this, base.holdingObj);
					base.holdingObj?.OnRelease(this);
					this.OnHeldConnectionBreak?.Invoke(this, base.holdingObj);
					this.OnReleased?.Invoke(this, base.holdingObj);
					ignoreMoveFrame = true;
				}
				BreakGrabConnection();
			}
		}

		public virtual void ForceReleaseGrab()
		{
			if (base.holdingObj != null)
			{
				this.OnForcedRelease?.Invoke(this, base.holdingObj);
				base.holdingObj?.ForceHandRelease(this);
			}
		}

		public virtual void ReleaseGrabLock()
		{
			ForceReleaseGrab();
		}

		public virtual void Squeeze()
		{
			this.OnSqueezed?.Invoke(this, base.holdingObj);
			base.holdingObj?.OnSqueeze(this);
			foreach (HandTriggerAreaEvents triggerEventArea in triggerEventAreas)
			{
				triggerEventArea.Squeeze(this);
			}
			squeezing = true;
		}

		public virtual void Unsqueeze()
		{
			squeezing = false;
			this.OnUnsqueezed?.Invoke(this, base.holdingObj);
			base.holdingObj?.OnUnsqueeze(this);
			foreach (HandTriggerAreaEvents triggerEventArea in triggerEventAreas)
			{
				triggerEventArea.Unsqueeze(this);
			}
		}

		public virtual void BreakGrabConnection(bool callEvent = true)
		{
			if (base.holdingObj != null)
			{
				if (squeezing)
				{
					base.holdingObj.OnUnsqueeze(this);
				}
				if (grabbing && base.holdingObj.body != null)
				{
					base.holdingObj.body.velocity = Vector3.zero;
					base.holdingObj.body.angularVelocity = Vector3.zero;
				}
				Finger[] array = fingers;
				foreach (Finger obj in array)
				{
					obj.SetCurrentFingerBend(obj.GetLastHitBend());
				}
				if (base.holdingObj.ignoreReleaseTime == 0f)
				{
					base.transform.position = base.holdingObj.body.transform.InverseTransformPoint(startHandGrabPosition);
					base.body.position = base.transform.position;
				}
				base.holdingObj.BreakHandConnection(this);
				lastHoldingObj = base.holdingObj;
				base.holdingObj = null;
			}
			velocityTracker.Disable(throwVelocityExpireTime);
			grabbed = false;
			base.grabPose = null;
			base.lookingAtObj = null;
			base.grabPositionOffset = Vector3.zero;
			base.grabRotationOffset = Quaternion.identity;
			grabRoutine = null;
			if (heldJoint != null)
			{
				Object.Destroy(heldJoint);
				heldJoint = null;
			}
		}

		public virtual void CreateGrabConnection(Grabbable grab, Vector3 handPos, Quaternion handRot, Vector3 grabPos, Quaternion grabRot, bool executeGrabEvents = false)
		{
			if (executeGrabEvents)
			{
				this.OnBeforeGrabbed?.Invoke(this, grab);
				grab.OnBeforeGrab(this);
			}
			base.transform.position = handPos;
			base.body.position = handPos;
			base.transform.rotation = handRot;
			base.body.rotation = handRot;
			grab.transform.position = grabPos;
			grab.body.position = grabPos;
			grab.transform.rotation = grabRot;
			grab.body.rotation = grabRot;
			base.grabPoint.parent = grab.transform;
			base.grabPoint.transform.position = handPos;
			base.grabPoint.transform.rotation = handRot;
			base.holdingObj = grab;
			base.grabPosition.transform.position = base.holdingObj.body.transform.position;
			base.grabPosition.transform.rotation = base.holdingObj.body.transform.rotation;
			if (base.holdingObj.grabType != HandGrabType.GrabbableToHand && grabType != GrabType.GrabbableToHand)
			{
				base.grabPositionOffset = base.transform.position - follow.transform.position;
				base.grabRotationOffset = Quaternion.Inverse(follow.transform.rotation) * base.transform.rotation;
			}
			if (base.holdingObj.GetSavedPose(out var pose) && pose.CanSetPose(this))
			{
				base.grabPose = pose.GetClosestPose(this);
				base.grabPose.SetHandPose(this);
			}
			if (executeGrabEvents)
			{
				this.OnGrabbed?.Invoke(this, base.holdingObj);
				base.holdingObj.OnGrab(this);
			}
			CreateJoint(base.holdingObj, base.holdingObj.jointBreakForce * (1f / Time.fixedUnscaledDeltaTime / 60f), float.PositiveInfinity);
		}

		public virtual void OnJointBreak(float breakForce)
		{
			if (heldJoint != null)
			{
				Object.Destroy(heldJoint);
				heldJoint = null;
			}
			if (base.holdingObj != null)
			{
				base.holdingObj.body.velocity /= 100f;
				base.holdingObj.body.angularVelocity /= 100f;
				this.OnGrabJointBreak?.Invoke(this, base.holdingObj);
				base.holdingObj?.OnHandJointBreak(this);
			}
		}

		public virtual void UpdateHighlight()
		{
			if (!usingHighlight || (int)highlightLayers == 0 || !(base.holdingObj == null) || IsGrabbing())
			{
				return;
			}
			if (HandClosestHit(out highlightHit, out var grabbable, reachDistance, highlightLayers) != Vector3.zero && grabbable != null && grabbable.CanGrab(this))
			{
				if (grabbable != base.lookingAtObj)
				{
					if (base.lookingAtObj != null)
					{
						this.OnStopHighlight?.Invoke(this, base.lookingAtObj);
						base.lookingAtObj.Unhighlight(this);
					}
					base.lookingAtObj = grabbable;
					this.OnHighlight?.Invoke(this, base.lookingAtObj);
					base.lookingAtObj.Highlight(this);
				}
			}
			else if (grabbable == null && base.lookingAtObj != null)
			{
				this.OnStopHighlight?.Invoke(this, base.lookingAtObj);
				base.lookingAtObj.Unhighlight(this);
				base.lookingAtObj = null;
			}
		}

		public RaycastHit GetHighlightHit()
		{
			highlightHit.point = base.grabPoint.position;
			highlightHit.normal = base.grabPoint.up;
			return highlightHit;
		}

		public void AutoPose(RaycastHit hit, Grabbable grabbable)
		{
			int layer = grabbable.gameObject.layer;
			int newLayer = LayerMask.NameToLayer(grabbingLayerName);
			grabbable.SetLayerRecursive(grabbable.transform, newLayer);
			Vector3 palmLocalPos = palmTransform.localPosition;
			Quaternion palmLocalRot = palmTransform.localRotation;
			for (int i = 0; i < 10; i++)
			{
				Calculate();
			}
			Finger[] array = fingers;
			for (int j = 0; j < array.Length; j++)
			{
				array[j].BendFingerUntilHit(fingerBendSteps, LayerMask.GetMask(grabbingLayerName));
			}
			grabbable.SetLayerRecursive(grabbable.transform, layer);
			void Align()
			{
				palmChild.position = base.transform.position;
				palmChild.rotation = base.transform.rotation;
				palmTransform.LookAt(hit.point, palmTransform.up);
				base.transform.position = palmChild.position;
				base.transform.rotation = palmChild.rotation;
				palmTransform.localPosition = palmLocalPos;
				palmTransform.localRotation = palmLocalRot;
			}
			void Calculate()
			{
				Align();
				Vector3 vector = hit.point - palmTransform.position;
				base.transform.position += vector;
				base.body.position = base.transform.position;
				palmCollider.enabled = true;
				if (Physics.ComputePenetration(hit.collider, hit.collider.transform.position, hit.collider.transform.rotation, palmCollider, palmCollider.transform.position, palmCollider.transform.rotation, out var direction, out var distance))
				{
					base.transform.position -= direction * distance / 2f;
					base.body.position = base.transform.position;
				}
				palmCollider.enabled = false;
				Align();
				base.transform.position -= palmTransform.forward * vector.magnitude / 3f;
				base.body.position = base.transform.position;
			}
		}

		public HandPoseData GetHandPose()
		{
			return new HandPoseData(this);
		}

		public HandPoseData GetHeldPose()
		{
			if ((bool)base.holdingObj)
			{
				return new HandPoseData(this, base.holdingObj);
			}
			return new HandPoseData(this);
		}

		public virtual void SetHeldPose(HandPoseData pose, Grabbable grabbable, bool createJoint = true)
		{
			pose.SetPose(this, grabbable.transform);
			if (createJoint)
			{
				base.holdingObj = grabbable;
				this.OnBeforeGrabbed?.Invoke(this, base.holdingObj);
				base.holdingObj.body.transform.position = base.transform.position;
				CreateJoint(base.holdingObj, base.holdingObj.jointBreakForce * (1f / Time.fixedUnscaledDeltaTime / 60f), float.PositiveInfinity);
				base.grabPoint.parent = base.holdingObj.transform;
				base.grabPoint.transform.position = base.transform.position;
				base.grabPoint.transform.rotation = base.transform.rotation;
				this.OnGrabbed?.Invoke(this, base.holdingObj);
				base.holdingObj.OnGrab(this);
				SetHandLocation(base.moveTo.position, base.moveTo.rotation);
				grabbed = true;
			}
		}

		public void SetHandPose(HandPoseData pose)
		{
			pose.SetPose(this);
		}

		public void SetHandPose(GrabbablePose pose)
		{
			pose.GetHandPoseData(this).SetPose(this);
		}

		public void UpdatePose(HandPoseData pose, float time)
		{
			if (handAnimateRoutine != null)
			{
				StopCoroutine(handAnimateRoutine);
			}
			if (base.gameObject.activeInHierarchy)
			{
				handAnimateRoutine = StartCoroutine(LerpHandPose(GetHandPose(), pose, time));
			}
		}

		public GrabbablePose GetGrabPose(Transform from, Grabbable grabbable)
		{
			GrabbablePose result = null;
			if (grabbable.GetSavedPose(out var pose) && pose.CanSetPose(this))
			{
				return pose.GetClosestPose(this);
			}
			return result;
		}

		public bool GetCurrentHeldGrabPose(Transform from, Grabbable grabbable, out GrabbablePose grabPose, out Transform relativeTo)
		{
			if (grabbable.GetSavedPose(out var pose) && pose.CanSetPose(this))
			{
				grabPose = pose.GetClosestPose(this);
				relativeTo = grabbable.transform;
				return true;
			}
			if (grabbable.GetSavedPose(out var pose2) && pose2.CanSetPose(this))
			{
				grabPose = pose2.GetClosestPose(this);
				relativeTo = from;
				return true;
			}
			grabPose = null;
			relativeTo = from;
			return false;
		}

		public Grabbable GetHeldGrabbable()
		{
			return base.holdingObj;
		}

		public Grabbable GetHeld()
		{
			return base.holdingObj;
		}

		public bool IsSqueezing()
		{
			return squeezing;
		}

		public void ResetGrabOffset()
		{
			base.grabPositionOffset = base.transform.position - follow.transform.position;
			base.grabRotationOffset = Quaternion.Inverse(follow.transform.rotation) * base.transform.rotation;
		}

		public void SetGrip(float grip)
		{
			triggerPoint = grip;
		}

		[ContextMenu("Set Pose - Relax Hand")]
		public void RelaxHand()
		{
			Finger[] array = fingers;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetFingerBend(gripOffset);
			}
		}

		[ContextMenu("Set Pose - Open Hand")]
		public void OpenHand()
		{
			Finger[] array = fingers;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetFingerBend(0f);
			}
		}

		[ContextMenu("Set Pose - Close Hand")]
		public void CloseHand()
		{
			Finger[] array = fingers;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetFingerBend(1f);
			}
		}

		[ContextMenu("Bend Fingers Until Hit")]
		public void ProceduralFingerBend()
		{
			ProceduralFingerBend(~LayerMask.GetMask(rightHandLayerName, leftHandLayerName));
		}

		public void ProceduralFingerBend(int layermask)
		{
			Finger[] array = fingers;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].BendFingerUntilHit(fingerBendSteps, layermask);
			}
		}

		public void ProceduralFingerBend(RaycastHit hit)
		{
			Finger[] array = fingers;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].BendFingerUntilHit(fingerBendSteps, hit.transform.gameObject.layer);
			}
		}

		public void PlayHapticVibration()
		{
			PlayHapticVibration(0.05f, 0.5f);
		}

		public void PlayHapticVibration(float duration)
		{
			PlayHapticVibration(duration, 0.5f);
		}

		public void PlayHapticVibration(float duration, float amp = 0.5f)
		{
			if (left)
			{
				HandControllerLink.handLeft.TryHapticImpulse(duration, amp);
			}
			else
			{
				HandControllerLink.handRight.TryHapticImpulse(duration, amp);
			}
		}

		[Button("Copy Pose", EButtonEnableMode.Always)]
		[ContextMenu("COPY POSE")]
		public void CopyPose()
		{
			if (copyFromHand != null)
			{
				if (copyFromHand.fingers.Length != fingers.Length)
				{
					Debug.LogError("Cannot copy pose because hand reference does not have the same number of fingers attached as this hand");
					return;
				}
				for (int i = 0; i < copyFromHand.fingers.Length; i++)
				{
					fingers[i].CopyPose(copyFromHand.fingers[i]);
				}
				Debug.Log("Auto Hand: Copied Hand Pose!");
			}
			else
			{
				Debug.LogError("Cannot copy pose because hand reference to copy from is not set");
			}
		}

		[Button("Save Open Pose", EButtonEnableMode.Always)]
		[ContextMenu("SAVE OPEN")]
		public void SaveOpenPose()
		{
			Finger[] array = fingers;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetMinPose();
			}
			Debug.Log("Auto Hand: Saved Open Hand Pose!");
		}

		[Button("Save Closed Pose", EButtonEnableMode.Always)]
		[ContextMenu("SAVE CLOSED")]
		public void SaveClosedPose()
		{
			Finger[] array = fingers;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetMaxPose();
			}
			Debug.Log("Auto Hand: Saved Closed Hand Pose!");
		}

		protected virtual void OnCollisionFirstEnter(GameObject collision)
		{
			if (collision.CanGetComponent<HandTouchEvent>(out var component))
			{
				component.Touch(this);
			}
		}

		protected virtual void OnCollisionLastExit(GameObject collision)
		{
			if (collision.CanGetComponent<HandTouchEvent>(out var component))
			{
				component.Untouch(this);
			}
		}

		protected virtual void OnTriggerFirstEnter(GameObject other)
		{
			CheckEnterPoseArea(other);
			if (other.CanGetComponent<HandTriggerAreaEvents>(out var component))
			{
				triggerEventAreas.Add(component);
				component.Enter(this);
			}
		}

		protected virtual void OnTriggerLastExit(GameObject other)
		{
			CheckExitPoseArea(other);
			if (other.CanGetComponent<HandTriggerAreaEvents>(out var component))
			{
				triggerEventAreas.Remove(component);
				component.Exit(this);
			}
		}

		private IEnumerator HighlightUpdate(float timestep)
		{
			if (left)
			{
				yield return new WaitForSecondsRealtime(timestep / 2f);
			}
			while (true)
			{
				if (usingHighlight)
				{
					UpdateHighlight();
				}
				yield return new WaitForSecondsRealtime(timestep);
			}
		}

		protected IEnumerator GrabObject(RaycastHit hit, Grabbable grab, GrabType grabType)
		{
			if (!CanGrab(grab))
			{
				yield break;
			}
			base.grabPoint.parent = hit.collider.transform;
			base.grabPoint.position = hit.point;
			base.grabPoint.up = hit.normal;
			while (grab.beingGrabbed)
			{
				yield return new WaitForEndOfFrame();
			}
			CancelPose();
			ClearPoseArea();
			base.grabPose = null;
			grabbing = true;
			base.holdingObj = grab;
			base.lookingAtObj = null;
			bool instantGrab = base.holdingObj.instantGrab || grabType == GrabType.InstantGrab;
			Grabbable startHoldingObj = base.holdingObj;
			base.body.velocity = Vector3.zero;
			base.body.angularVelocity = Vector3.zero;
			foreach (Collider heldIgnoreCollider in base.holdingObj.heldIgnoreColliders)
			{
				HandIgnoreCollider(heldIgnoreCollider, ignore: true);
			}
			this.OnBeforeGrabbed?.Invoke(this, base.holdingObj);
			base.holdingObj.OnBeforeGrab(this);
			startHandGrabPosition = base.holdingObj.transform.InverseTransformPoint(base.transform.position);
			if (base.holdingObj == null)
			{
				CancelGrab();
				yield break;
			}
			Vector3 startGrabbablePosition = base.holdingObj.transform.position;
			Quaternion startGrabbableRotation = base.holdingObj.transform.rotation;
			startGrabDist = Vector3.Distance(palmTransform.position, base.grabPoint.position);
			GrabbablePose grabbablePose = (base.grabPose = GetGrabPose(hit.collider.transform, base.holdingObj));
			HandPoseData startGrabPose;
			if ((bool)grabbablePose)
			{
				startGrabPose = new HandPoseData(this, base.grabPose.transform);
			}
			else
			{
				startGrabPose = new HandPoseData(this, base.grabPoint);
				base.transform.position -= palmTransform.forward * 0.08f;
				base.body.position = base.transform.position;
				hit.point = base.grabPoint.position;
				hit.normal = base.grabPoint.up;
				AutoPose(hit, base.holdingObj);
			}
			float adjustedGrabTime = GetGrabTime();
			instantGrab = instantGrab || adjustedGrabTime == 0f;
			if (grabType == GrabType.GrabbableToHand && base.holdingObj.singleHandOnly && base.holdingObj.HeldCount() > 0)
			{
				base.holdingObj.ForceHandRelease(base.holdingObj.GetHeldBy()[0]);
			}
			if (!instantGrab)
			{
				Transform grabTarget = ((base.grabPose != null) ? base.grabPose.transform : base.grabPoint);
				HandPoseData postGrabPose = ((base.grabPose == null) ? new HandPoseData(this, base.grabPoint) : base.grabPose.GetHandPoseData(this));
				Vector3 endGrabbablePosition = base.transform.InverseTransformPoint(base.holdingObj.transform.position);
				Quaternion endGrabbableRotation = Quaternion.Inverse(palmTransform.transform.rotation) * base.holdingObj.transform.rotation;
				Finger[] array = fingers;
				foreach (Finger finger in array)
				{
					finger.SetFingerBend(gripOffset + Mathf.Clamp01(finger.GetCurrentBend() / 4f));
				}
				openHandPose = GetHandPose();
				if (grabType == GrabType.HandToGrabbable || (grabType == GrabType.GrabbableToHand && (base.holdingObj.HeldCount() > 0 || !base.holdingObj.parentOnGrab)))
				{
					for (float i2 = 0f; i2 < adjustedGrabTime; i2 += Time.deltaTime)
					{
						if (!(base.holdingObj != null))
						{
							continue;
						}
						i2 += base.holdingObj.GetVelocity().magnitude * Time.deltaTime * 5f;
						i2 += followVel.magnitude * Time.deltaTime * 7f;
						if (i2 < adjustedGrabTime)
						{
							float num = Mathf.Clamp01(i2 / adjustedGrabTime);
							float num2 = 0.5f;
							float num3 = 1.5f;
							if (num < num2)
							{
								HandPoseData.LerpPose(startGrabPose, openHandPose, grabCurve.Evaluate(num * 1f / num2)).SetFingerPose(this, grabTarget);
							}
							else
							{
								HandPoseData.LerpPose(openHandPose, postGrabPose, grabCurve.Evaluate((num - num2) * (1f / (1f - num2)))).SetFingerPose(this, grabTarget);
							}
							HandPoseData.LerpPose(startGrabPose, postGrabPose, num * num3).SetPosition(this, grabTarget);
							base.body.position = base.transform.position;
							base.body.rotation = base.transform.rotation;
							if (base.holdingObj.body != null)
							{
								base.holdingObj.body.angularVelocity *= 0.5f;
							}
							yield return new WaitForEndOfFrame();
						}
					}
					if (base.holdingObj != null && base.holdingObj.singleHandOnly && base.holdingObj.HeldCount() > 0)
					{
						base.holdingObj.ForceHandRelease(base.holdingObj.GetHeldBy()[0]);
						if (base.holdingObj.body != null)
						{
							base.holdingObj.body.velocity = Vector3.zero;
							base.holdingObj.body.angularVelocity = Vector3.zero;
						}
					}
				}
				else if (grabType == GrabType.GrabbableToHand)
				{
					base.holdingObj.ActivateRigidbody();
					bool useGravity = true;
					if (base.holdingObj.body != null)
					{
						useGravity = base.holdingObj.body.useGravity;
						base.holdingObj.body.useGravity = false;
					}
					for (float i2 = 0f; i2 < adjustedGrabTime; i2 += Time.deltaTime)
					{
						if (base.holdingObj != null)
						{
							float num4 = Mathf.Clamp01(i2 / adjustedGrabTime);
							float num5 = 0.5f;
							if (num4 < num5)
							{
								HandPoseData.LerpPose(startGrabPose, openHandPose, grabCurve.Evaluate(num4 * 1f / num5)).SetFingerPose(this, grabTarget);
							}
							else
							{
								HandPoseData.LerpPose(openHandPose, postGrabPose, grabCurve.Evaluate((num4 - num5) * (1f / (1f - num5)))).SetFingerPose(this, grabTarget);
							}
							SetMoveTo();
							SetHandLocation(base.moveTo.position, base.moveTo.rotation);
							base.body.position = base.transform.position;
							base.body.rotation = base.transform.rotation;
							base.holdingObj.body.transform.position = Vector3.Lerp(startGrabbablePosition, base.transform.TransformPoint(endGrabbablePosition), grabCurve.Evaluate(num4 * 2f));
							base.holdingObj.body.transform.rotation = Quaternion.Lerp(startGrabbableRotation, palmTransform.rotation * endGrabbableRotation, grabCurve.Evaluate(num4 * 2f));
							if (base.holdingObj.body != null)
							{
								base.holdingObj.body.position = base.holdingObj.body.transform.position;
								base.holdingObj.body.rotation = base.holdingObj.body.transform.rotation;
								base.holdingObj.body.velocity = Vector3.zero;
								base.holdingObj.body.angularVelocity = Vector3.zero;
							}
							i2 += followVel.magnitude * Time.deltaTime * 2f;
							yield return new WaitForEndOfFrame();
						}
					}
					if (base.holdingObj != null && base.holdingObj.body != null)
					{
						base.holdingObj.body.useGravity = useGravity;
					}
					else if (startHoldingObj.body != null)
					{
						startHoldingObj.body.useGravity = useGravity;
					}
				}
				if (base.holdingObj != null)
				{
					if (base.grabPose != null)
					{
						base.grabPose.SetHandPose(this, grabTarget);
					}
					else
					{
						postGrabPose.SetPose(this, grabTarget);
					}
					base.body.position = base.transform.position;
					base.body.rotation = base.transform.rotation;
					if (base.holdingObj.body != null)
					{
						base.holdingObj.body.position = base.holdingObj.body.transform.position;
						base.holdingObj.body.rotation = base.holdingObj.body.transform.rotation;
					}
				}
			}
			else
			{
				if (base.holdingObj.singleHandOnly && base.holdingObj.HeldCount() > 0)
				{
					base.holdingObj.ForceHandRelease(base.holdingObj.GetHeldBy()[0]);
					if (base.holdingObj.body != null)
					{
						base.holdingObj.body.velocity = Vector3.zero;
						base.holdingObj.body.angularVelocity = Vector3.zero;
					}
				}
				if (base.grabPose != null)
				{
					base.grabPose.SetHandPose(this, base.grabPose.transform);
				}
			}
			if (base.holdingObj == null)
			{
				CancelGrab();
				yield break;
			}
			base.holdingObj.ActivateRigidbody();
			CreateJoint(base.holdingObj, base.holdingObj.jointBreakForce * (1f / Time.fixedUnscaledDeltaTime / 60f), float.PositiveInfinity);
			SetMoveTo();
			base.grabPoint.transform.position = base.transform.position;
			base.grabPoint.transform.rotation = base.transform.rotation;
			base.grabPosition.position = base.holdingObj.transform.position;
			base.grabPosition.rotation = base.holdingObj.transform.rotation;
			this.OnGrabbed?.Invoke(this, base.holdingObj);
			base.holdingObj.OnGrab(this);
			if (!instantGrab || !base.holdingObj.parentOnGrab)
			{
				base.grabPositionOffset = base.transform.position - follow.transform.position;
				base.grabRotationOffset = Quaternion.Inverse(follow.transform.rotation) * base.transform.rotation;
			}
			if (instantGrab && base.holdingObj.parentOnGrab)
			{
				SetMoveTo();
				SetHandLocation(base.moveTo.position, base.moveTo.rotation);
			}
			if (base.holdingObj == null)
			{
				CancelGrab();
				yield break;
			}
			grabbed = true;
			grabbing = false;
			startHoldingObj.beingGrabbed = false;
			grabRoutine = null;
			void CancelGrab()
			{
				BreakGrabConnection();
				if ((bool)startHoldingObj)
				{
					if (startHoldingObj.body != null)
					{
						startHoldingObj.body.velocity = Vector3.zero;
						startHoldingObj.body.angularVelocity = Vector3.zero;
					}
					startHoldingObj.beingGrabbed = false;
				}
				grabbing = false;
				grabRoutine = null;
			}
		}

		protected void CancelPose()
		{
			if (handAnimateRoutine != null)
			{
				StopCoroutine(handAnimateRoutine);
			}
			handAnimateRoutine = null;
			base.grabPose = null;
		}

		protected virtual IEnumerator LerpHandPose(HandPoseData fromPose, HandPoseData toPose, float totalTime)
		{
			for (float timePassed = 0f; timePassed < totalTime; timePassed += Time.deltaTime)
			{
				SetHandPose(HandPoseData.LerpPose(fromPose, toPose, Mathf.Pow(timePassed / totalTime, 0.5f)));
				yield return new WaitForEndOfFrame();
			}
			SetHandPose(HandPoseData.LerpPose(fromPose, toPose, 1f));
			handAnimateRoutine = null;
		}

		protected virtual void CheckEnterPoseArea(GameObject other)
		{
			if ((bool)base.holdingObj || !usingPoseAreas || !other.activeInHierarchy || !other || !other.CanGetComponent<HandPoseArea>(out var component))
			{
				return;
			}
			for (int i = 0; i < component.poseAreas.Length; i++)
			{
				if (component.poseIndex != poseIndex)
				{
					continue;
				}
				if (component.HasPose(left) && (handPoseArea == null || handPoseArea != component))
				{
					if (handPoseArea == null)
					{
						preHandPoseAreaPose = GetHandPose();
					}
					else if (handPoseArea != null)
					{
						TryRemoveHandPoseArea(handPoseArea);
					}
					handPoseArea = component;
					handPoseArea?.OnHandEnter?.Invoke(this);
					if (base.holdingObj == null)
					{
						UpdatePose(handPoseArea.GetHandPoseData(left), handPoseArea.transitionTime);
					}
				}
				break;
			}
		}

		protected virtual void CheckExitPoseArea(GameObject other)
		{
			if (usingPoseAreas && other.gameObject.activeInHierarchy && other.CanGetComponent<HandPoseArea>(out var component))
			{
				TryRemoveHandPoseArea(component);
			}
		}

		internal void TryRemoveHandPoseArea(HandPoseArea poseArea)
		{
			if (!(handPoseArea != null) || !handPoseArea.gameObject.Equals(poseArea.gameObject))
			{
				return;
			}
			try
			{
				if (base.holdingObj == null)
				{
					if (handPoseArea != null)
					{
						UpdatePose(preHandPoseAreaPose, handPoseArea.transitionTime);
					}
					handPoseArea?.OnHandExit?.Invoke(this);
					handPoseArea = null;
				}
				else if (base.holdingObj != null)
				{
					handPoseArea?.OnHandExit?.Invoke(this);
					handPoseArea = null;
				}
			}
			catch (MissingReferenceException)
			{
				handPoseArea = null;
				SetHandPose(preHandPoseAreaPose);
			}
		}

		private void ClearPoseArea()
		{
			if (handPoseArea != null)
			{
				handPoseArea.OnHandExit?.Invoke(this);
			}
			handPoseArea = null;
		}

		internal virtual void RemoveHandTriggerArea(HandTriggerAreaEvents handTrigger)
		{
			handTrigger.Exit(this);
			triggerEventAreas.Remove(handTrigger);
		}
	}
}
