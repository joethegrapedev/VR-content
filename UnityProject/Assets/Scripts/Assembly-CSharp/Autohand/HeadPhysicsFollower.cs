using UnityEngine;

namespace Autohand
{
	[RequireComponent(typeof(Rigidbody))]
	public class HeadPhysicsFollower : MonoBehaviour
	{
		[Header("References")]
		public Camera headCamera;

		public Transform trackingContainer;

		public Transform followBody;

		[Header("Follow Settings")]
		public float followStrength = 50f;

		[Tooltip("The maximum allowed distance from the body for the headCamera to still move")]
		internal SphereCollider headCollider;

		private Vector3 startHeadPos;

		private bool started;

		private Transform _moveTo;

		internal Rigidbody body;

		private CollisionTracker collisionTracker;

		private float lastUpdateTime;

		private Transform moveTo
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
					_moveTo.transform.rotation = base.transform.rotation;
					_moveTo.rotation = base.transform.rotation;
					_moveTo.name = "HEAD FOLLOW POINT";
					_moveTo.parent = AutoHandExtensions.transformParent;
				}
				return _moveTo;
			}
		}

		public void Start()
		{
			if (collisionTracker == null)
			{
				collisionTracker = base.gameObject.AddComponent<CollisionTracker>();
				collisionTracker.disableTriggersTracking = true;
			}
			body = GetComponent<Rigidbody>();
			base.gameObject.layer = LayerMask.NameToLayer("HandPlayer");
			base.transform.position = headCamera.transform.position;
			base.transform.rotation = headCamera.transform.rotation;
			headCollider = GetComponent<SphereCollider>();
			startHeadPos = headCamera.transform.position;
		}

		internal void Init()
		{
			if (collisionTracker == null)
			{
				collisionTracker = base.gameObject.AddComponent<CollisionTracker>();
				collisionTracker.disableTriggersTracking = true;
			}
			base.gameObject.layer = LayerMask.NameToLayer("HandPlayer");
			base.transform.position = headCamera.transform.position;
			base.transform.rotation = headCamera.transform.rotation;
			headCollider = GetComponent<SphereCollider>();
			startHeadPos = headCamera.transform.position;
		}

		protected void FixedUpdate()
		{
			moveTo.position = headCamera.transform.position;
			if (startHeadPos.y != headCamera.transform.position.y && !started)
			{
				started = true;
				body.position = headCamera.transform.position;
			}
			if (started)
			{
				MoveTo();
			}
		}

		public bool Started()
		{
			return started;
		}

		internal virtual void MoveTo()
		{
			moveTo.position = headCamera.transform.position;
			Vector3 position = moveTo.position;
			float num = Vector3.Distance(position, base.transform.position);
			Vector3 velocity = (position - base.transform.position).normalized * followStrength * num;
			body.velocity = velocity;
			lastUpdateTime = Time.realtimeSinceStartup;
		}

		protected virtual void Update()
		{
			if (CollisionCount() == 0 && moveTo != null && !body.isKinematic)
			{
				float num = Time.realtimeSinceStartup - lastUpdateTime;
				MoveTo();
				base.transform.position = Vector3.MoveTowards(base.transform.position, moveTo.position, body.velocity.magnitude * num);
				body.velocity = Vector3.MoveTowards(body.velocity, Vector3.zero, body.velocity.magnitude * num);
				body.position = base.transform.position;
			}
			lastUpdateTime = Time.realtimeSinceStartup;
		}

		public int CollisionCount()
		{
			return collisionTracker.collisionCount;
		}
	}
}
