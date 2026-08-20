using System.Collections.Generic;
using UnityEngine;

namespace Autohand
{
	public class CollisionTracker : MonoBehaviour
	{
		public bool disableCollisionTracking;

		public bool disableTriggersTracking;

		public int collisionCount => collisionObjects.Count;

		public int triggerCount => triggerObjects.Count;

		public List<GameObject> triggerObjects { get; protected set; } = new List<GameObject>();

		public List<int> triggerObjectsCount { get; protected set; } = new List<int>();

		public List<GameObject> collisionObjects { get; protected set; } = new List<GameObject>();

		public List<int> collisionObjectsCount { get; protected set; } = new List<int>();

		public event CollisionEvent OnCollisionFirstEnter;

		public event CollisionEvent OnCollisionLastExit;

		public event CollisionEvent OnTriggerFirstEnter;

		public event CollisionEvent OnTriggeLastExit;

		public void Clear()
		{
			triggerObjects.Clear();
			triggerObjectsCount.Clear();
			collisionObjects.Clear();
			collisionObjectsCount.Clear();
		}

		protected virtual void OnDisable()
		{
			for (int i = 0; i < collisionObjects.Count; i++)
			{
				if ((bool)collisionObjects[i])
				{
					this.OnCollisionLastExit?.Invoke(collisionObjects[i]);
				}
			}
			for (int j = 0; j < triggerObjects.Count; j++)
			{
				if ((bool)triggerObjects[j])
				{
					this.OnTriggeLastExit?.Invoke(triggerObjects[j]);
				}
			}
			collisionObjects.Clear();
			collisionObjectsCount.Clear();
			triggerObjects.Clear();
			triggerObjectsCount.Clear();
		}

		private void FixedUpdate()
		{
			CheckCollisions();
		}

		private void CheckCollisions()
		{
			if (!disableCollisionTracking)
			{
				for (int i = 0; i < collisionObjects.Count; i++)
				{
					if (collisionObjects[i] == null)
					{
						collisionObjects.RemoveAt(i);
						collisionObjectsCount.RemoveAt(i);
					}
					else if (!collisionObjects[i].activeInHierarchy)
					{
						this.OnCollisionLastExit?.Invoke(collisionObjects[i]);
						collisionObjects.RemoveAt(i);
						collisionObjectsCount.RemoveAt(i);
					}
				}
			}
			if (disableTriggersTracking)
			{
				return;
			}
			for (int j = 0; j < triggerObjects.Count; j++)
			{
				if (triggerObjects[j] == null)
				{
					triggerObjects.RemoveAt(j);
					triggerObjectsCount.RemoveAt(j);
				}
				else if (!triggerObjects[j].activeInHierarchy)
				{
					this.OnTriggeLastExit?.Invoke(triggerObjects[j]);
					triggerObjects.RemoveAt(j);
					triggerObjectsCount.RemoveAt(j);
				}
			}
		}

		protected virtual void OnCollisionEnter(Collision collision)
		{
			if (!disableCollisionTracking)
			{
				if (!collisionObjects.Contains(collision.collider.gameObject))
				{
					this.OnCollisionFirstEnter?.Invoke(collision.collider.gameObject);
					collisionObjects.Add(collision.collider.gameObject);
					collisionObjectsCount.Add(1);
				}
				else
				{
					collisionObjectsCount[collisionObjects.IndexOf(collision.collider.gameObject)]++;
				}
			}
		}

		protected virtual void OnCollisionExit(Collision collision)
		{
			if (!disableCollisionTracking && collisionObjects.Contains(collision.collider.gameObject))
			{
				int index = collisionObjects.IndexOf(collision.collider.gameObject);
				if (--collisionObjectsCount[index] == 0)
				{
					this.OnCollisionLastExit?.Invoke(collision.collider.gameObject);
					collisionObjectsCount.RemoveAt(index);
					collisionObjects.Remove(collision.collider.gameObject);
				}
			}
		}

		protected virtual void OnTriggerEnter(Collider other)
		{
			if (!disableTriggersTracking)
			{
				if (!triggerObjects.Contains(other.gameObject))
				{
					this.OnTriggerFirstEnter?.Invoke(other.gameObject);
					triggerObjects.Add(other.gameObject);
					triggerObjectsCount.Add(1);
				}
				else
				{
					triggerObjectsCount[triggerObjects.IndexOf(other.gameObject)]++;
				}
			}
		}

		protected virtual void OnTriggerExit(Collider other)
		{
			if (!disableTriggersTracking && triggerObjects.Contains(other.gameObject))
			{
				int index = triggerObjects.IndexOf(other.gameObject);
				triggerObjectsCount[index]--;
				if (triggerObjectsCount[index] == 0)
				{
					this.OnTriggeLastExit?.Invoke(other.gameObject);
					triggerObjectsCount.RemoveAt(index);
					triggerObjects.Remove(other.gameObject);
				}
			}
		}
	}
}
