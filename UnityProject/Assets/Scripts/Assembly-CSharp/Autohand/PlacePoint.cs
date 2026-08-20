using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

namespace Autohand
{
	[HelpURL("https://earnestrobot.notion.site/Place-Points-e6361a414928450dbb53d76fd653cf9a")]
	public class PlacePoint : MonoBehaviour
	{
		[AutoHeader("Place Point", 0, 0)]
		public bool ignoreMe;

		[AutoSmallHeader("Place Settings", 0, 0)]
		public bool showPlaceSettings = true;

		[Tooltip("Snaps an object to the point at start, leave empty for no target")]
		public Grabbable startPlaced;

		[Tooltip("This will make the point place the object as soon as it enters the radius, instead of on release")]
		public Transform placedOffset;

		[Tooltip("The radius of the place point (relative to scale)")]
		public float placeRadius = 0.1f;

		[Space]
		[Tooltip("This will make the point place the object as soon as it enters the radius, instead of on release")]
		public bool parentOnPlace = true;

		[Tooltip("This will make the point place the object as soon as it enters the radius, instead of on release")]
		public bool forcePlace;

		[Tooltip("If true and will force hand to release on place when force place is called. If false the hand will attempt to keep the connection to the held object (but can still break due to max distances/break forces)")]
		public bool forceHandRelease = true;

		[Space]
		[Tooltip("Whether or not the placed object should be disabled on placement (this will hide the placed object and leave the place point active for a new object)")]
		public bool destroyObjectOnPlace;

		[Tooltip("Whether or not the placed object should have its rigidbody disabled on place, good for parenting placed objects under dynamic objects")]
		public bool disableRigidbodyOnPlace;

		[Tooltip("Whether or not the grabbable should be disabled on place")]
		public bool disableGrabOnPlace;

		[Tooltip("Whether or not this place point should be disabled on placement. It will maintain its connection and can no longer accept new items. Causes less overhead if true")]
		public bool disablePlacePointOnPlace;

		[Space]
		[Tooltip("If true and will force release on place")]
		[DisableIf("disableRigidbodyOnPlace")]
		public bool makePlacedKinematic = true;

		[DisableIf("disableRigidbodyOnPlace")]
		[Tooltip("The rigidbody to attach the placed grabbable to - leave empty means no joint")]
		public Rigidbody placedJointLink;

		[DisableIf("disableRigidbodyOnPlace")]
		public float jointBreakForce = 1000f;

		[AutoSmallHeader("Place Requirements", 0, 0)]
		public bool showPlaceRequirements = true;

		[Tooltip("Whether the placeNames should compare names or tags")]
		public PlacePointNameType nameCompareType;

		[Tooltip("Will allow placement for any grabbable with a name containing this array of strings, leave blank for any grabbable allowed")]
		public string[] placeNames;

		[Tooltip("Will prevent placement for any name containing this array of strings")]
		public string[] blacklistNames;

		[Tooltip("(Unless empty) Will only allow placement any object contained here")]
		public List<Grabbable> onlyAllows;

		[Tooltip("Will NOT allow placement any object contained here")]
		public List<Grabbable> dontAllows;

		[Tooltip("The layer that this place point will check for placeable objects, if none will default to Grabbable")]
		public LayerMask placeLayers;

		[Tooltip("Whether or not to only allow placement of an object while it's being held (or released)")]
		public bool heldPlaceOnly;

		[Space]
		[AutoToggleHeader("Show Events", 0, 0)]
		public bool showEvents = true;

		[ShowIf("showEvents")]
		public UnityPlacePointEvent OnPlace;

		[ShowIf("showEvents")]
		public UnityPlacePointEvent OnRemove;

		[ShowIf("showEvents")]
		public UnityPlacePointEvent OnHighlight;

		[ShowIf("showEvents")]
		public UnityPlacePointEvent OnStopHighlight;

		public PlacePointEvent OnPlaceEvent;

		public PlacePointEvent OnRemoveEvent;

		public PlacePointEvent OnHighlightEvent;

		public PlacePointEvent OnStopHighlightEvent;

		[HideInInspector]
		public Vector3 radiusOffset;

		protected FixedJoint joint;

		protected float removalDistance = 0.05f;

		protected float lastPlacedTime;

		protected Vector3 placePosition;

		protected Transform originParent;

		protected bool placingFrame;

		protected CollisionDetectionMode placedObjDetectionMode;

		private float tickRate = 0.02f;

		private Collider[] collidersNonAlloc = new Collider[30];

		private Coroutine checkRoutine;

		private int lastOverlapCount;

		private Grabbable tempGrabbable;

		public Grabbable highlightingObj { get; protected set; }

		public Grabbable placedObject { get; protected set; }

		public Grabbable lastPlacedObject { get; protected set; }

		protected virtual void Start()
		{
			if (placedOffset == null)
			{
				placedOffset = base.transform;
			}
			if ((int)placeLayers == 0)
			{
				placeLayers = LayerMask.GetMask(Hand.grabbableLayerNameDefault);
			}
			SetStartPlaced();
		}

		protected virtual void OnEnable()
		{
			checkRoutine = StartCoroutine(CheckPlaceObjectLoop());
		}

		protected virtual void OnDisable()
		{
			StopCoroutine(checkRoutine);
		}

		public virtual bool CanPlace(Grabbable placeObj)
		{
			if (placedObject != null)
			{
				return false;
			}
			if (heldPlaceOnly && placeObj.HeldCount() == 0)
			{
				return false;
			}
			if (onlyAllows.Count > 0 && !onlyAllows.Contains(placeObj))
			{
				return false;
			}
			if (dontAllows.Count > 0 && dontAllows.Contains(placeObj))
			{
				return false;
			}
			if (placeNames.Length == 0 && blacklistNames.Length == 0)
			{
				return true;
			}
			if (blacklistNames.Length != 0)
			{
				string[] array = blacklistNames;
				foreach (string value in array)
				{
					if (nameCompareType == PlacePointNameType.name && placeObj.name.Contains(value))
					{
						return false;
					}
					if (nameCompareType == PlacePointNameType.tag && placeObj.CompareTag(value))
					{
						return false;
					}
				}
			}
			if (placeNames.Length != 0)
			{
				string[] array = placeNames;
				foreach (string value2 in array)
				{
					if (placeObj.name.Contains(value2))
					{
						if (nameCompareType == PlacePointNameType.name && placeObj.name.Contains(value2))
						{
							return true;
						}
						if (nameCompareType == PlacePointNameType.tag && placeObj.CompareTag(value2))
						{
							return true;
						}
					}
				}
				return false;
			}
			return true;
		}

		protected virtual IEnumerator CheckPlaceObjectLoop()
		{
			float scale = Mathf.Abs((base.transform.lossyScale.x < base.transform.lossyScale.y) ? base.transform.lossyScale.x : base.transform.lossyScale.y);
			scale = Mathf.Abs((scale < base.transform.lossyScale.z) ? scale : base.transform.lossyScale.z);
			CheckPlaceObject(placeRadius, scale);
			yield return new WaitForSeconds(UnityEngine.Random.Range(0f, tickRate));
			while (base.gameObject.activeInHierarchy)
			{
				CheckPlaceObject(placeRadius, scale);
				yield return new WaitForSeconds(tickRate);
			}
		}

		private void CheckPlaceObject(float radius, float scale)
		{
			if (!disablePlacePointOnPlace && !disableRigidbodyOnPlace && placedObject != null && !IsStillOverlapping(placedObject, scale))
			{
				Remove(placedObject);
			}
			if (placedObject == null && highlightingObj == null)
			{
				int num = Physics.OverlapSphereNonAlloc(placedOffset.position + base.transform.rotation * radiusOffset, radius * scale, collidersNonAlloc, placeLayers);
				if (num != lastOverlapCount)
				{
					for (int i = 0; i < num; i++)
					{
						if (collidersNonAlloc[i].gameObject.HasGrabbable(out tempGrabbable) && CanPlace(tempGrabbable))
						{
							Highlight(tempGrabbable);
						}
					}
				}
				lastOverlapCount = num;
			}
			else if (highlightingObj != null && !IsStillOverlapping(highlightingObj, scale))
			{
				StopHighlight(highlightingObj);
			}
		}

		public virtual void TryPlace(Grabbable placeObj)
		{
			if (CanPlace(placeObj))
			{
				Place(placeObj);
			}
		}

		public virtual void Place(Grabbable placeObj)
		{
			if (placedObject != null)
			{
				return;
			}
			if (placeObj.placePoint != null && placeObj.placePoint != this)
			{
				placeObj.placePoint.Remove(placeObj);
			}
			placedObject = placeObj;
			placedObject.SetPlacePoint(this);
			if ((forceHandRelease || disableRigidbodyOnPlace) && placeObj.HeldCount() > 0)
			{
				placeObj.ForceHandsRelease();
			}
			placingFrame = true;
			originParent = placeObj.transform.parent;
			placeObj.transform.position = placedOffset.position;
			placeObj.transform.rotation = placedOffset.rotation;
			if (placeObj.body != null)
			{
				placeObj.body.position = placeObj.transform.position;
				placeObj.body.rotation = placeObj.transform.rotation;
				placeObj.body.velocity = Vector3.zero;
				placeObj.body.angularVelocity = Vector3.zero;
				placedObjDetectionMode = placeObj.body.collisionDetectionMode;
				if (makePlacedKinematic)
				{
					placeObj.body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
					placeObj.body.isKinematic = makePlacedKinematic;
				}
				if (placedJointLink != null)
				{
					joint = placedJointLink.gameObject.AddComponent<FixedJoint>();
					joint.connectedBody = placeObj.body;
					joint.breakForce = jointBreakForce;
					joint.breakTorque = jointBreakForce;
					joint.connectedMassScale = 1f;
					joint.massScale = 1f;
					joint.enableCollision = false;
					joint.enablePreprocessing = false;
				}
			}
			placeObj.OnGrabEvent = (HandGrabEvent)Delegate.Combine(placeObj.OnGrabEvent, new HandGrabEvent(OnPlacedObjectGrabbed));
			placeObj.OnReleaseEvent = (HandGrabEvent)Delegate.Combine(placeObj.OnReleaseEvent, new HandGrabEvent(OnPlacedObjectReleased));
			StopHighlight(placeObj);
			placePosition = placedObject.transform.position;
			placeObj.OnPlacePointAddEvent?.Invoke(this, placeObj);
			OnPlaceEvent?.Invoke(this, placeObj);
			OnPlace?.Invoke(this, placeObj);
			lastPlacedTime = Time.time;
			if (parentOnPlace)
			{
				placedObject.body.transform.parent = base.transform;
			}
			if (disableRigidbodyOnPlace)
			{
				placeObj.DeactivateRigidbody();
			}
			if (disablePlacePointOnPlace)
			{
				base.enabled = false;
			}
			if (disableGrabOnPlace || disablePlacePointOnPlace)
			{
				placeObj.isGrabbable = false;
			}
			if (destroyObjectOnPlace)
			{
				UnityEngine.Object.Destroy(placedObject);
			}
		}

		public void Remove()
		{
			if (placedObject != null)
			{
				Remove(placedObject);
			}
		}

		public virtual void Remove(Grabbable placeObj)
		{
			if (placeObj == null || placeObj != placedObject || disablePlacePointOnPlace)
			{
				return;
			}
			if (placeObj.body != null)
			{
				if (makePlacedKinematic)
				{
					placeObj.body.isKinematic = false;
				}
				placeObj.body.collisionDetectionMode = placedObjDetectionMode;
			}
			placeObj.OnGrabEvent = (HandGrabEvent)Delegate.Remove(placeObj.OnGrabEvent, new HandGrabEvent(OnPlacedObjectGrabbed));
			placeObj.OnReleaseEvent = (HandGrabEvent)Delegate.Remove(placeObj.OnReleaseEvent, new HandGrabEvent(OnPlacedObjectReleased));
			if ((!placeObj.parentOnGrab || (placeObj.HeldCount() <= 0 && !placeObj.beingGrabbed)) && parentOnPlace && !placeObj.BeingDestroyed())
			{
				placeObj.transform.parent = originParent;
			}
			placedObject.OnPlacePointRemoveEvent?.Invoke(this, highlightingObj);
			OnRemoveEvent?.Invoke(this, placeObj);
			OnRemove?.Invoke(this, placeObj);
			Highlight(placeObj);
			if (disableRigidbodyOnPlace)
			{
				placeObj.ActivateRigidbody();
			}
			lastPlacedObject = placedObject;
			placedObject = null;
			if (joint != null)
			{
				UnityEngine.Object.Destroy(joint);
				joint = null;
			}
		}

		internal virtual void Highlight(Grabbable from)
		{
			if (highlightingObj == null)
			{
				from.SetPlacePoint(this);
				highlightingObj = from;
				highlightingObj.OnPlacePointHighlightEvent?.Invoke(this, highlightingObj);
				OnHighlightEvent?.Invoke(this, from);
				OnHighlight?.Invoke(this, from);
				if (placedObject == null && forcePlace)
				{
					Place(from);
				}
			}
		}

		internal virtual void StopHighlight(Grabbable from)
		{
			if (highlightingObj != null)
			{
				highlightingObj.OnPlacePointUnhighlightEvent?.Invoke(this, highlightingObj);
				highlightingObj = null;
				OnStopHighlightEvent?.Invoke(this, from);
				OnStopHighlight?.Invoke(this, from);
				if (placedObject == null)
				{
					from.SetPlacePoint(null);
				}
			}
		}

		protected bool IsStillOverlapping(Grabbable from, float scale = 1f)
		{
			int num = Physics.OverlapSphereNonAlloc(placedOffset.position + placedOffset.rotation * radiusOffset, placeRadius * scale, collidersNonAlloc, placeLayers);
			for (int i = 0; i < num; i++)
			{
				if (collidersNonAlloc[i].attachedRigidbody == from.body)
				{
					return true;
				}
			}
			return false;
		}

		public virtual void SetStartPlaced()
		{
			if (startPlaced != null)
			{
				startPlaced.SetPlacePoint(this);
				Highlight(startPlaced);
				Place(startPlaced);
			}
		}

		public Grabbable GetPlacedObject()
		{
			return placedObject;
		}

		protected virtual void OnPlacedObjectGrabbed(Hand pHand, Grabbable pGrabbable)
		{
			if (makePlacedKinematic)
			{
				pGrabbable.body.isKinematic = false;
			}
		}

		protected virtual void OnPlacedObjectReleased(Hand pHand, Grabbable pGrabbable)
		{
			if (makePlacedKinematic)
			{
				Place(pGrabbable);
			}
		}

		protected virtual void OnJointBreak(float breakForce)
		{
			if (placedObject != null)
			{
				Remove(placedObject);
			}
		}

		private void OnDrawGizmos()
		{
			if (placedOffset == null)
			{
				placedOffset = base.transform;
			}
			Gizmos.color = Color.white;
			float num = Mathf.Abs((base.transform.lossyScale.x < base.transform.lossyScale.y) ? base.transform.lossyScale.x : base.transform.lossyScale.y);
			num = Mathf.Abs((num < base.transform.lossyScale.z) ? num : base.transform.lossyScale.z);
			Gizmos.DrawWireSphere(base.transform.rotation * radiusOffset + placedOffset.position, placeRadius * num);
		}
	}
}
