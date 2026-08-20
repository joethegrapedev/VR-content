using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Autohand
{
	public class Stabbable : MonoBehaviour
	{
		public Rigidbody body;

		public Grabbable grabbable;

		[Tooltip("The index that must match the stabbers index to allow stabbing")]
		public int stabIndex;

		public int maxStabbers = 1;

		public float positionDamper = 1000f;

		public float rotationDamper = 1000f;

		public bool parentOnStab = true;

		[Header("Events")]
		public UnityEvent StartStab;

		public UnityEvent EndStab;

		public StabEvent StartStabEvent;

		public StabEvent EndStabEvent;

		private int currentStabs;

		private List<Stabber> stabbing;

		private Transform stabParent;

		public void Start()
		{
			stabbing = new List<Stabber>();
			StartStabEvent = (StabEvent)Delegate.Combine(StartStabEvent, (StabEvent)delegate
			{
				StartStab?.Invoke();
			});
			EndStabEvent = (StabEvent)Delegate.Combine(EndStabEvent, (StabEvent)delegate
			{
				EndStab?.Invoke();
			});
			if (!(grabbable != null))
			{
				return;
			}
			Grabbable obj = grabbable;
			obj.OnReleaseEvent = (HandGrabEvent)Delegate.Combine(obj.OnReleaseEvent, (HandGrabEvent)delegate
			{
				if (stabbing.Count > 0)
				{
					body.transform.parent = stabParent;
				}
			});
		}

		public virtual void OnStab(Stabber stabber)
		{
			currentStabs++;
			stabbing.Add(stabber);
			StartStabEvent?.Invoke(stabber, this);
			stabParent = body.transform.parent;
		}

		public virtual void OnEndStab(Stabber stabber)
		{
			currentStabs--;
			stabbing.Remove(stabber);
			EndStabEvent?.Invoke(stabber, this);
		}

		public virtual bool CanStab(Stabber stabber)
		{
			if (currentStabs < maxStabbers)
			{
				return stabber.stabIndex == stabIndex;
			}
			return false;
		}

		public int StabbedCount()
		{
			return stabbing.Count;
		}

		private void OnDrawGizmosSelected()
		{
			if (!body && (bool)GetComponent<Rigidbody>())
			{
				body = GetComponent<Rigidbody>();
			}
		}
	}
}
