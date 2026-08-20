using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Autohand
{
	[DefaultExecutionOrder(-5)]
	public class GrabbableBase : MonoBehaviour
	{
		[AutoHeader("Grabbable", 0, 0)]
		public bool ignoreMe;

		[Tooltip("The physics body to connect this colliders grab to - if left empty will default to local body")]
		public Rigidbody body;

		[Tooltip("A copy of the mesh will be created and slighly scaled and this material will be applied to create a highlight effect with options")]
		public Material hightlightMaterial;

		[HideInInspector]
		public bool isGrabbable = true;

		private PlacePoint _placePoint;

		internal bool ignoreParent;

		protected List<Hand> heldBy = new List<Hand>();

		protected bool hightlighting;

		protected GameObject highlightObj;

		protected PlacePoint lastPlacePoint;

		protected Transform originalParent;

		protected Vector3 lastCenterOfMassPos;

		protected Quaternion lastCenterOfMassRot;

		protected CollisionDetectionMode detectionMode;

		protected RigidbodyInterpolation startInterpolation;

		protected internal bool beingGrabbed;

		protected bool heldBodyJointed;

		protected bool wasIsGrabbable;

		protected bool beingDestroyed;

		protected int originalLayer;

		protected Coroutine resetLayerRoutine;

		protected List<GrabbableChild> grabChildren = new List<GrabbableChild>();

		protected List<Transform> jointedParents = new List<Transform>();

		protected GrabbablePoseCombiner poseCombiner;

		protected List<Grabbable> jointedGrabbables = new List<Grabbable>();

		protected float lastUpdateTime;

		protected bool rigidbodyDeactivated;

		protected SaveRigidbodyData rigidbodyData;

		private CollisionTracker _collisionTracker;

		protected Hand ignoringHand;

		protected List<Collider> grabColliders = new List<Collider>();

		public PlacePoint placePoint
		{
			get
			{
				return _placePoint;
			}
			protected set
			{
				_placePoint = value;
			}
		}

		public CollisionTracker collisionTracker
		{
			get
			{
				if (_collisionTracker == null && !(_collisionTracker = GetComponent<CollisionTracker>()))
				{
					_collisionTracker = base.gameObject.AddComponent<CollisionTracker>();
					_collisionTracker.disableTriggersTracking = true;
				}
				return _collisionTracker;
			}
			protected set
			{
				if (_collisionTracker != null)
				{
					Object.Destroy(_collisionTracker);
				}
				_collisionTracker = value;
			}
		}

		protected virtual void Awake()
		{
			if (base.gameObject.layer == LayerMask.NameToLayer("Default") || LayerMask.LayerToName(base.gameObject.layer) == "")
			{
				base.gameObject.layer = LayerMask.NameToLayer(Hand.grabbableLayerNameDefault);
			}
			if (heldBy == null)
			{
				heldBy = new List<Hand>();
			}
			if (body == null)
			{
				if ((bool)GetComponent<Rigidbody>())
				{
					body = GetComponent<Rigidbody>();
				}
				else
				{
					Debug.LogError("RIGIDBODY MISSING FROM GRABBABLE: " + base.transform.name + " \nPlease add/attach a rigidbody", this);
				}
			}
			originalLayer = base.gameObject.layer;
			originalParent = body.transform.parent;
			detectionMode = body.collisionDetectionMode;
			startInterpolation = body.interpolation;
			SetCollidersRecursive(body.transform);
		}

		protected virtual void Start()
		{
			if (!base.gameObject.CanGetComponent<GrabbablePoseCombiner>(out poseCombiner))
			{
				poseCombiner = base.gameObject.AddComponent<GrabbablePoseCombiner>();
			}
			GetPoseSaves(base.transform);
			void GetPoseSaves(Transform obj)
			{
				if (!obj.CanGetComponent<Grabbable>(out var component) || !(component != this))
				{
					GrabbablePose[] components = obj.GetComponents<GrabbablePose>();
					for (int i = 0; i < components.Length; i++)
					{
						poseCombiner.AddPose(components[i]);
					}
					for (int j = 0; j < obj.childCount; j++)
					{
						GetPoseSaves(obj.GetChild(j));
					}
				}
			}
		}

		protected virtual void FixedUpdate()
		{
			if (heldBy.Count > 0 && body != null)
			{
				lastCenterOfMassRot = body.transform.rotation;
				lastCenterOfMassPos = body.transform.position;
			}
		}

		protected virtual void OnDisable()
		{
			if (resetLayerRoutine != null)
			{
				StopCoroutine(resetLayerRoutine);
				resetLayerRoutine = null;
			}
		}

		internal void SetPlacePoint(PlacePoint point)
		{
			placePoint = point;
		}

		internal void SetGrabbableChild(GrabbableChild child)
		{
			if (!grabChildren.Contains(child))
			{
				grabChildren.Add(child);
			}
		}

		public void DeactivateRigidbody()
		{
			if (body != null)
			{
				if (body != null)
				{
					rigidbodyData = new SaveRigidbodyData(body);
				}
				body = null;
				rigidbodyDeactivated = true;
			}
		}

		public void ActivateRigidbody()
		{
			if (rigidbodyDeactivated)
			{
				rigidbodyDeactivated = false;
				body = rigidbodyData.ReloadRigidbody();
			}
		}

		protected int GetOriginalLayer()
		{
			return originalLayer;
		}

		internal void SetLayerRecursive(Transform obj, int oldLayer, int newLayer)
		{
			for (int i = 0; i < grabChildren.Count; i++)
			{
				if (grabChildren[i].gameObject.layer == oldLayer)
				{
					grabChildren[i].gameObject.layer = newLayer;
				}
			}
			SetChildrenLayers(obj);
			void SetChildrenLayers(Transform transform)
			{
				if (transform.gameObject.layer == oldLayer)
				{
					transform.gameObject.layer = newLayer;
				}
				for (int j = 0; j < transform.childCount; j++)
				{
					SetChildrenLayers(transform.GetChild(j));
				}
			}
		}

		internal void SetLayerRecursive(Transform obj, int newLayer)
		{
			SetLayerRecursive(obj, obj.gameObject.layer, newLayer);
		}

		protected IEnumerator IgnoreHandCollision(float time, Hand hand)
		{
			if (ignoringHand != null)
			{
				IgnoreHand(ignoringHand, ignore: false);
			}
			ignoringHand = hand;
			if (time > 0f)
			{
				IgnoreHand(hand, ignore: true);
				yield return new WaitForSeconds(time);
			}
			IgnoreHand(hand, ignore: false);
			ignoringHand = null;
			resetLayerRoutine = null;
		}

		public bool GetSavedPose(out GrabbablePoseCombiner pose)
		{
			if (poseCombiner != null && poseCombiner.PoseCount() > 0)
			{
				pose = poseCombiner;
				return true;
			}
			pose = null;
			return false;
		}

		public bool HasCustomPose()
		{
			return poseCombiner.PoseCount() > 0;
		}

		public void IgnoreHand(Hand hand, bool ignore)
		{
			foreach (Collider grabCollider in grabColliders)
			{
				hand.HandIgnoreCollider(grabCollider, ignore);
			}
		}

		private void SetCollidersRecursive(Transform obj)
		{
			Collider[] components = obj.GetComponents<Collider>();
			foreach (Collider item in components)
			{
				grabColliders.Add(item);
			}
			for (int j = 0; j < obj.childCount; j++)
			{
				SetCollidersRecursive(obj.GetChild(j));
			}
		}

		protected void ResetRigidbody()
		{
			if (body != null)
			{
				body.collisionDetectionMode = detectionMode;
				body.interpolation = startInterpolation;
			}
		}

		public bool BeingDestroyed()
		{
			return beingDestroyed;
		}

		public void DebugBreak()
		{
		}
	}
}
