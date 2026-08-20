using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace Autohand
{
	[HelpURL("https://earnestrobot.notion.site/Grabbables-9308c564e60848a882eb23e9778ee2b6")]
	[DefaultExecutionOrder(-5)]
	public class Grabbable : GrabbableBase
	{
		[Tooltip("This will copy the given grabbables settings to this grabbable when applied")]
		[OnValueChanged("EditorCopyGrabbable")]
		public Grabbable CopySettings;

		[Header("Grab Settings")]
		[Tooltip("Which hand this can be held by")]
		public HandGrabType grabType;

		[Tooltip("Which hand this can be held by")]
		public HandType handType;

		[Tooltip("Whether or not this can be grabbed with more than one hand")]
		public bool singleHandOnly;

		[ShowIf("singleHandOnly")]
		[Tooltip("if false single handed items cannot be passes back and forth on grab")]
		public bool allowHeldSwapping = true;

		[Tooltip("Will the item automatically return the hand on grab - good for saved poses, bad for heavy things")]
		public bool instantGrab;

		[DisableIf("instantGrab")]
		[Tooltip("If true (and using HandToGrabbable) the hand will only return to the follow while moving. Good for picking up objects without disrupting the things around them - you can change the speed of the hand return on the hand through the gentleGrabSpeed value")]
		public bool useGentleGrab;

		[Tooltip("Creates an offset an grab so the hand will not return to the hand on grab - Good for statically jointed grabbable objects")]
		public bool maintainGrabOffset;

		[Tooltip("Experimental - ignores weight of held object while held")]
		public bool ignoreWeight;

		[Tooltip("This will NOT parent the object under the hands on grab. This will parent the object to the parents of the hand, which allow you to move the hand parent object smoothly while holding an item, but will also allow you to move items that are very heavy - recommended for all objects that aren't very heavy or jointed to other rigidbodies")]
		public bool parentOnGrab = true;

		[Header("Release Settings")]
		[Tooltip("How much to multiply throw by for this grabbable when releasing - 0-1 for no or reduced throw strength")]
		[FormerlySerializedAs("throwMultiplyer")]
		public float throwPower = 1f;

		[Tooltip("The required force to break the fixedJoint\n Turn this to \"infinity\" to disable (Might cause jitter)\nIdeal value depends on hand mass and velocity settings")]
		public float jointBreakForce = 3500f;

		[AutoSmallHeader("Advanced Settings", 0, 0)]
		public bool showAdvancedSettings = true;

		[Tooltip("Adds and links a GrabbableChild to each child with a collider on start - So the hand can grab them")]
		public bool makeChildrenGrabbable = true;

		[Min(0f)]
		[Tooltip("I.E. Grab Prioirty - BIGGER IS BETTER - divides highlight distance by this when calculating which object to grab. Hands always grab closest object to palm")]
		public float grabPriorityWeight = 1f;

		[Tooltip("The number of seconds that the hand collision should ignore the released object\n (Good for increased placement precision and resolves clipping errors)")]
		[Min(0f)]
		public float ignoreReleaseTime = 0.5f;

		[Space]
		[Tooltip("Offsets the grabbable by this much when being held")]
		public Vector3 heldPositionOffset;

		[Tooltip("Offsets the grabbable by this many degrees when being held")]
		public Vector3 heldRotationOffset;

		[Space]
		[Min(0f)]
		[Tooltip("The joint that connects the hand and the grabbable. Defaults to the joint in AutoHand/Resources/DefaultJoint.prefab if empty")]
		public ConfigurableJoint customGrabJoint;

		[Space]
		[Tooltip("For the special use case of having grabbable objects with physics jointed peices move properly while being held")]
		public List<Rigidbody> jointedBodies = new List<Rigidbody>();

		[Tooltip("For the special use case of having things connected to the grabbable that the hand should ignore while being held (good for doors and drawers) -> for always active use the [GrabbableIgnoreHands] Component")]
		public List<Collider> heldIgnoreColliders = new List<Collider>();

		[Space]
		[Tooltip("Whether or not the break call made only when holding with multiple hands - if this is false the break event can be called by forcing an object into a static collider")]
		public bool pullApartBreakOnly = true;

		[AutoToggleHeader("Show Events", 0, 0)]
		public bool showEvents = true;

		[Space]
		[ShowIf("showEvents")]
		public UnityHandGrabEvent onGrab = new UnityHandGrabEvent();

		[ShowIf("showEvents")]
		public UnityHandGrabEvent onRelease = new UnityHandGrabEvent();

		[ShowIf("showEvents")]
		[Space]
		[Space]
		public UnityHandGrabEvent onSqueeze = new UnityHandGrabEvent();

		[ShowIf("showEvents")]
		public UnityHandGrabEvent onUnsqueeze = new UnityHandGrabEvent();

		[Space]
		[Space]
		[ShowIf("showEvents")]
		public UnityHandGrabEvent onHighlight = new UnityHandGrabEvent();

		[ShowIf("showEvents")]
		public UnityHandGrabEvent onUnhighlight = new UnityHandGrabEvent();

		[Space]
		[Space]
		[ShowIf("showEvents")]
		public UnityHandGrabEvent OnJointBreak = new UnityHandGrabEvent();

		[HideInInspector]
		[Tooltip("Lock hand in place on grab (This is a legacy setting, set hand kinematic on grab/release instead)")]
		public bool lockHandOnGrab;

		public HandGrabEvent OnBeforeGrabEvent;

		public HandGrabEvent OnGrabEvent;

		public HandGrabEvent OnReleaseEvent;

		public HandGrabEvent OnJointBreakEvent;

		public HandGrabEvent OnSqueezeEvent;

		public HandGrabEvent OnUnsqueezeEvent;

		public HandGrabEvent OnHighlightEvent;

		public HandGrabEvent OnUnhighlightEvent;

		public PlacePointEvent OnPlacePointHighlightEvent;

		public PlacePointEvent OnPlacePointUnhighlightEvent;

		public PlacePointEvent OnPlacePointAddEvent;

		public PlacePointEvent OnPlacePointRemoveEvent;

		private bool ignoreInterpolation;

		private Dictionary<Material, List<GameObject>> highlightObjs = new Dictionary<Material, List<GameObject>>();

		public bool wasForceReleased { get; internal set; }

		public Hand lastHeldBy { get; protected set; }

		public float throwMultiplyer
		{
			get
			{
				return throwPower;
			}
			set
			{
				throwPower = value;
			}
		}

		protected override void Start()
		{
			base.Start();
		}

		protected new virtual void Awake()
		{
			if (makeChildrenGrabbable)
			{
				MakeChildrenGrabbable();
			}
			base.Awake();
			for (int i = 0; i < jointedBodies.Count; i++)
			{
				jointedParents.Add(jointedBodies[i].transform.parent ?? null);
				if (jointedBodies[i].gameObject.HasGrabbable(out var grabbable) && jointedGrabbables.Contains(grabbable))
				{
					jointedGrabbables.Add(grabbable);
				}
			}
		}

		private void Update()
		{
			UpdateHeldInterpolation();
		}

		protected override void FixedUpdate()
		{
			base.FixedUpdate();
			if (wasIsGrabbable && !isGrabbable && !base.enabled)
			{
				ForceHandsRelease();
			}
			wasIsGrabbable = isGrabbable || base.enabled;
			ignoreInterpolation = false;
			lastUpdateTime = Time.fixedTime;
		}

		protected virtual void OnDestroy()
		{
			beingDestroyed = true;
			if (resetLayerRoutine != null)
			{
				if (ignoringHand != null)
				{
					IgnoreHand(ignoringHand, ignore: false);
				}
				StopCoroutine(resetLayerRoutine);
				resetLayerRoutine = null;
			}
			if (heldBy.Count != 0)
			{
				ForceHandsRelease();
			}
			MakeChildrenUngrabbable();
			if (base.placePoint != null && !base.placePoint.disablePlacePointOnPlace)
			{
				base.placePoint.Remove(this);
			}
			UnityEngine.Object.Destroy(poseCombiner);
		}

		internal bool ShouldInterpolate()
		{
			bool flag = false;
			for (int i = 0; i < heldBy.Count; i++)
			{
				flag = flag || heldBy[i].IsGrabbing();
			}
			if (!rigidbodyDeactivated && !flag && !ignoreInterpolation && heldBy.Count > 0 && jointedBodies.Count == 0 && parentOnGrab && CollisionCount() == 0 && !body.isKinematic && body.constraints == RigidbodyConstraints.None && HeldCount() == heldBy.Count)
			{
				return body.mass / 4f < heldBy[0].body.mass;
			}
			return false;
		}

		internal void UpdateHeldInterpolation()
		{
			lastUpdateTime = Time.time;
		}

		internal void IgnoreInterpolationForOneFixedUpdate()
		{
			ignoreInterpolation = true;
		}

		internal void IgnoreColliders(Collider bodyCapsule, bool ignore = true)
		{
			foreach (Collider grabCollider in grabColliders)
			{
				Physics.IgnoreCollision(bodyCapsule, grabCollider, ignore);
			}
		}

		private void TryCreateHighlight(Material customMat, Hand hand)
		{
			Material highlightMat = ((customMat != null) ? customMat : hightlightMaterial);
			highlightMat = ((highlightMat != null) ? highlightMat : hand.defaultHighlight);
			if (highlightMat != null && !highlightObjs.ContainsKey(highlightMat))
			{
				highlightObjs.Add(highlightMat, new List<GameObject>());
				AddHighlightObject(base.transform);
			}
			bool AddHighlightObject(Transform obj)
			{
				if (obj.CanGetComponent<Grabbable>(out var component) && component != this)
				{
					return false;
				}
				if (highlightObjs[highlightMat].Contains(obj.gameObject))
				{
					return true;
				}
				for (int i = 0; i < obj.childCount && AddHighlightObject(obj.GetChild(i)); i++)
				{
				}
				if (obj.CanGetComponent<MeshRenderer>(out var component2))
				{
					GameObject gameObject = new GameObject();
					gameObject.transform.parent = obj;
					gameObject.transform.localPosition = Vector3.zero;
					gameObject.transform.localRotation = Quaternion.identity;
					gameObject.transform.localScale = Vector3.one * 1.001f;
					gameObject.AddComponent<MeshFilter>().sharedMesh = obj.GetComponent<MeshFilter>().sharedMesh;
					MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
					Material[] array = new Material[component2.materials.Length];
					for (int j = 0; j < array.Length; j++)
					{
						array[j] = highlightMat;
					}
					meshRenderer.materials = array;
					highlightObjs[highlightMat].Add(gameObject);
				}
				return true;
			}
		}

		private void DestroyHighlightCopy()
		{
		}

		private void ToggleHighlight(Hand hand, Material customMat, bool enableHighlight)
		{
			Material material = ((customMat != null) ? customMat : hightlightMaterial);
			material = ((material != null) ? material : hand.defaultHighlight);
			if (material != null && highlightObjs.ContainsKey(material))
			{
				for (int i = 0; i < highlightObjs[material].Count; i++)
				{
					highlightObjs[material][i].SetActive(enableHighlight);
				}
			}
		}

		internal virtual void Highlight(Hand hand, Material customMat = null)
		{
			if (!hightlighting)
			{
				hightlighting = true;
				onHighlight?.Invoke(hand, this);
				OnHighlightEvent?.Invoke(hand, this);
				TryCreateHighlight(customMat, hand);
				ToggleHighlight(hand, customMat, enableHighlight: true);
			}
		}

		internal virtual void Unhighlight(Hand hand, Material customMat = null)
		{
			if (hightlighting)
			{
				onUnhighlight?.Invoke(hand, this);
				OnUnhighlightEvent?.Invoke(hand, this);
				hightlighting = false;
				ToggleHighlight(hand, customMat, enableHighlight: false);
			}
		}

		internal virtual void OnSqueeze(Hand hand)
		{
			OnSqueezeEvent?.Invoke(hand, this);
			onSqueeze?.Invoke(hand, this);
		}

		internal virtual void OnUnsqueeze(Hand hand)
		{
			OnUnsqueezeEvent?.Invoke(hand, this);
			onUnsqueeze?.Invoke(hand, this);
		}

		internal virtual void OnBeforeGrab(Hand hand)
		{
			OnBeforeGrabEvent?.Invoke(hand, this);
			Unhighlight(hand);
			beingGrabbed = true;
			if (resetLayerRoutine != null)
			{
				if (ignoringHand != null)
				{
					IgnoreHand(ignoringHand, ignore: false);
				}
				StopCoroutine(resetLayerRoutine);
				resetLayerRoutine = null;
			}
			resetLayerRoutine = StartCoroutine(IgnoreHandCollision(hand.maxGrabTime, hand));
		}

		internal virtual void OnGrab(Hand hand)
		{
			if (rigidbodyDeactivated)
			{
				ActivateRigidbody();
			}
			if (lockHandOnGrab)
			{
				hand.body.isKinematic = true;
			}
			body.collisionDetectionMode = (body.isKinematic ? CollisionDetectionMode.ContinuousSpeculative : CollisionDetectionMode.ContinuousDynamic);
			body.interpolation = RigidbodyInterpolation.None;
			body.solverIterations = 200;
			body.solverVelocityIterations = 200;
			if (parentOnGrab)
			{
				body.transform.parent = hand.transform.parent;
				foreach (Rigidbody jointedBody in jointedBodies)
				{
					jointedBody.transform.parent = hand.transform.parent;
					if (jointedBody.gameObject.HasGrabbable(out var grabbable))
					{
						grabbable.heldBodyJointed = true;
					}
				}
			}
			if (ignoreWeight)
			{
				if (!base.gameObject.CanGetComponent<WeightlessFollower>(out var component) || singleHandOnly)
				{
					component = base.gameObject.AddComponent<WeightlessFollower>();
				}
				component?.Set(hand, this);
			}
			base.collisionTracker.enabled = true;
			base.placePoint?.Remove(this);
			heldBy?.Add(hand);
			onGrab?.Invoke(hand, this);
			OnGrabEvent?.Invoke(hand, this);
			wasForceReleased = false;
			beingGrabbed = false;
		}

		public virtual bool CanGrab(HandBase hand)
		{
			if (base.enabled && isGrabbable)
			{
				if (handType != HandType.both && (handType != HandType.left || !hand.left))
				{
					if (handType == HandType.right)
					{
						return !hand.left;
					}
					return false;
				}
				return true;
			}
			return false;
		}

		internal virtual void OnRelease(Hand hand)
		{
			if (!heldBy.Contains(hand))
			{
				return;
			}
			bool flag = base.placePoint != null && base.placePoint.CanPlace(this);
			BreakHandConnection(hand);
			if (body != null && heldBy.Count == 0)
			{
				body.velocity = hand.ThrowVelocity() * throwMultiplyer;
				Vector3 angularVelocity = hand.ThrowAngularVelocity();
				if (!float.IsNaN(angularVelocity.x) && !float.IsNaN(angularVelocity.y) && !float.IsNaN(angularVelocity.z))
				{
					body.angularVelocity = angularVelocity;
				}
			}
			OnReleaseEvent?.Invoke(hand, this);
			onRelease?.Invoke(hand, this);
			Unhighlight(hand);
			if (base.placePoint != null && flag)
			{
				base.placePoint.Place(this);
			}
		}

		internal virtual void BreakHandConnection(Hand hand)
		{
			if (!heldBy.Remove(hand))
			{
				return;
			}
			if (lockHandOnGrab)
			{
				hand.body.isKinematic = false;
			}
			if (base.gameObject.activeInHierarchy && !beingDestroyed)
			{
				if (resetLayerRoutine != null)
				{
					if (ignoringHand != null)
					{
						IgnoreHand(ignoringHand, ignore: false);
					}
					StopCoroutine(resetLayerRoutine);
					resetLayerRoutine = null;
				}
				resetLayerRoutine = StartCoroutine(IgnoreHandCollision(ignoreReleaseTime, hand));
			}
			foreach (Collider heldIgnoreCollider in heldIgnoreColliders)
			{
				hand.HandIgnoreCollider(heldIgnoreCollider, ignore: false);
			}
			if (HeldCount() == 0)
			{
				beingGrabbed = false;
				ResetGrabbableAfterRlease();
			}
			if (body != null)
			{
				body.solverIterations = Physics.defaultSolverIterations;
				body.solverVelocityIterations = Physics.defaultSolverVelocityIterations;
			}
			base.collisionTracker.enabled = false;
			lastHeldBy = hand;
		}

		public virtual void HandsRelease()
		{
			for (int num = heldBy.Count - 1; num >= 0; num--)
			{
				heldBy[num].Release();
			}
		}

		public virtual void HandRelease(Hand hand)
		{
			if (heldBy.Contains(hand))
			{
				hand.Release();
			}
		}

		public virtual void ForceHandsRelease()
		{
			for (int num = heldBy.Count - 1; num >= 0; num--)
			{
				wasForceReleased = true;
				ForceHandRelease(heldBy[num]);
			}
		}

		public virtual void ForceHandRelease(Hand hand)
		{
			if (heldBy.Contains(hand))
			{
				float num = throwPower;
				throwPower = 0f;
				wasForceReleased = true;
				hand.Release();
				throwPower = num;
			}
		}

		public virtual void OnHandJointBreak(Hand hand)
		{
			if (heldBy.Contains(hand))
			{
				if (body != null)
				{
					body.WakeUp();
					body.velocity *= 0f;
					body.angularVelocity *= 0f;
				}
				if (!pullApartBreakOnly)
				{
					OnJointBreakEvent?.Invoke(hand, this);
					OnJointBreak?.Invoke(hand, this);
				}
				if (pullApartBreakOnly && HeldCount() > 1)
				{
					OnJointBreakEvent?.Invoke(hand, this);
					OnJointBreak?.Invoke(hand, this);
				}
				ForceHandRelease(hand);
				if (heldBy.Count > 0)
				{
					heldBy[0].SetHandLocation(heldBy[0].moveTo.position, heldBy[0].transform.rotation);
				}
			}
		}

		public List<Hand> GetHeldBy()
		{
			return heldBy;
		}

		public int HeldCount(bool includedJointedCount = true)
		{
			int num = heldBy.Count;
			if (includedJointedCount)
			{
				for (int i = 0; i < jointedGrabbables.Count; i++)
				{
					num += jointedGrabbables[i].heldBy.Count;
				}
			}
			return num;
		}

		public bool IsHeld()
		{
			return heldBy.Count > 0;
		}

		public bool BeingGrabbed()
		{
			return beingGrabbed;
		}

		public void PlayHapticVibration()
		{
			foreach (Hand item in heldBy)
			{
				item.PlayHapticVibration();
			}
		}

		public void PlayHapticVibration(float duration = 0.025f)
		{
			foreach (Hand item in heldBy)
			{
				item.PlayHapticVibration(duration);
			}
		}

		public void PlayHapticVibration(float duration, float amp = 0.5f)
		{
			foreach (Hand item in heldBy)
			{
				item.PlayHapticVibration(duration, amp);
			}
		}

		public Vector3 GetVelocity()
		{
			if (body == null)
			{
				return Vector3.zero;
			}
			return lastCenterOfMassPos - body.position;
		}

		public Vector3 GetAngularVelocity()
		{
			(body.rotation * Quaternion.Inverse(lastCenterOfMassRot)).ToAngleAxis(out var angle, out var axis);
			angle *= MathF.PI / 180f;
			return 1f / Time.fixedDeltaTime * angle / 1.2f * axis;
		}

		public void SetParentOnGrab(bool parentOnGrab)
		{
			this.parentOnGrab = parentOnGrab;
		}

		public void AddJointedBody(Rigidbody body)
		{
			jointedBodies.Add(body);
			if (body.gameObject.HasGrabbable(out var grabbable))
			{
				jointedParents.Add(grabbable.originalParent);
			}
			else
			{
				jointedParents.Add(body.transform.parent);
			}
			if (!(base.transform.parent != originalParent))
			{
				return;
			}
			if (grabbable != null)
			{
				if (grabbable.HeldCount() == 0)
				{
					grabbable.transform.parent = base.transform.parent;
				}
				grabbable.heldBodyJointed = true;
			}
			else
			{
				grabbable.transform.parent = base.transform.parent;
			}
		}

		public void RemoveJointedBody(Rigidbody body)
		{
			int index = jointedBodies.IndexOf(body);
			if (jointedBodies[index].gameObject.HasGrabbable(out var grabbable))
			{
				if (grabbable.HeldCount() == 0)
				{
					grabbable.transform.parent = grabbable.originalParent;
				}
				grabbable.heldBodyJointed = false;
			}
			else
			{
				jointedBodies[index].transform.parent = jointedParents[index];
			}
			jointedBodies.RemoveAt(index);
			jointedParents.RemoveAt(index);
		}

		public void DoDestroy()
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}

		public int CollisionCount()
		{
			return base.collisionTracker.collisionObjects.Count;
		}

		public int JointedCollisionCount()
		{
			int num = 0;
			for (int i = 0; i < jointedGrabbables.Count; i++)
			{
				num += jointedGrabbables[i].HeldCount();
			}
			return num;
		}

		private void MakeChildrenGrabbable()
		{
			for (int i = 0; i < base.transform.childCount; i++)
			{
				AddChildGrabbableRecursive(base.transform.GetChild(i));
			}
			void AddChildGrabbableRecursive(Transform obj)
			{
				if (obj.CanGetComponent<Collider>(out var component) && !component.isTrigger && !obj.CanGetComponent<Grabbable>(out var component2) && !obj.CanGetComponent<GrabbableChild>(out var _) && !obj.CanGetComponent<PlacePoint>(out var _))
				{
					GrabbableChild grabbableChild = obj.gameObject.AddComponent<GrabbableChild>();
					grabbableChild.gameObject.layer = originalLayer;
					grabbableChild.grabParent = this;
				}
				for (int j = 0; j < obj.childCount; j++)
				{
					if (!obj.CanGetComponent<Grabbable>(out component2))
					{
						AddChildGrabbableRecursive(obj.GetChild(j));
					}
				}
			}
		}

		private void MakeChildrenUngrabbable()
		{
			for (int i = 0; i < base.transform.childCount; i++)
			{
				RemoveChildGrabbableRecursive(base.transform.GetChild(i));
			}
			void RemoveChildGrabbableRecursive(Transform obj)
			{
				if ((bool)obj.GetComponent<GrabbableChild>() && obj.GetComponent<GrabbableChild>().grabParent == this)
				{
					UnityEngine.Object.Destroy(obj.gameObject.GetComponent<GrabbableChild>());
				}
				for (int j = 0; j < obj.childCount; j++)
				{
					RemoveChildGrabbableRecursive(obj.GetChild(j));
				}
			}
		}

		internal void ResetGrabbableAfterRlease()
		{
			if (beingDestroyed)
			{
				return;
			}
			ResetRigidbody();
			if (body != null && !heldBodyJointed && (base.placePoint == null || !(base.placePoint.placedObject == this) || !base.placePoint.parentOnPlace))
			{
				body.transform.parent = originalParent;
			}
			for (int i = 0; i < jointedBodies.Count; i++)
			{
				if (jointedBodies[i].gameObject.HasGrabbable(out var grabbable))
				{
					if (grabbable.HeldCount() == 0)
					{
						grabbable.transform.parent = grabbable.originalParent;
					}
					grabbable.heldBodyJointed = false;
				}
				else if (!heldBodyJointed)
				{
					jointedBodies[i].transform.parent = jointedParents[i];
				}
			}
		}

		public bool IsHolding(Rigidbody body)
		{
			foreach (Hand item in heldBy)
			{
				if (item.body == body)
				{
					return true;
				}
			}
			return false;
		}

		public bool IsHolding(Hand hand)
		{
			foreach (Hand item in heldBy)
			{
				if (item == hand)
				{
					return true;
				}
			}
			return false;
		}
	}
}
