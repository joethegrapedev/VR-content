using System;
using UnityEngine;

namespace Autohand
{
	[RequireComponent(typeof(Grabbable))]
	public class Climbable : MonoBehaviour
	{
		public Vector3 axis = Vector3.one;

		public Stabber stabber;

		private void Start()
		{
			if (stabber != null)
			{
				Stabber obj = stabber;
				obj.StartStabEvent = (StabEvent)Delegate.Combine(obj.StartStabEvent, (StabEvent)delegate
				{
					base.enabled = true;
				});
				Stabber obj2 = stabber;
				obj2.EndStabEvent = (StabEvent)Delegate.Combine(obj2.EndStabEvent, (StabEvent)delegate
				{
					base.enabled = false;
				});
			}
		}
	}
}
