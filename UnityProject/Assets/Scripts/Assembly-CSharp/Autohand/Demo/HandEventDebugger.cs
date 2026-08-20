using UnityEngine;

namespace Autohand.Demo
{
	[RequireComponent(typeof(Hand))]
	public class HandEventDebugger : MonoBehaviour
	{
		public bool showSqueezeEvents = true;

		public bool showHighlightEvents = true;

		private void OnEnable()
		{
			Hand component = GetComponent<Hand>();
			component.OnBeforeGrabbed += delegate(Hand hand, Grabbable grabbable)
			{
				Debug.Log(hand.name + " BEFORE GRAB EVENT", this);
			};
			component.OnGrabbed += delegate(Hand hand, Grabbable grabbable)
			{
				Debug.Log(hand.name + " GRAB EVENT", this);
			};
			component.OnReleased += delegate(Hand hand, Grabbable grabbable)
			{
				Debug.Log(hand.name + " RELEASE EVENT", this);
			};
			component.OnGrabJointBreak += delegate(Hand hand, Grabbable grabbable)
			{
				Debug.Log(hand.name + " JOINT BREAK EVENT", this);
			};
			if (showSqueezeEvents)
			{
				component.OnSqueezed += delegate(Hand hand, Grabbable grabbable)
				{
					Debug.Log(hand.name + " SQUEEZE EVENT", this);
				};
			}
			if (showSqueezeEvents)
			{
				component.OnUnsqueezed += delegate(Hand hand, Grabbable grabbable)
				{
					Debug.Log(hand.name + " UNSQUEEZE EVENT", this);
				};
			}
			if (showHighlightEvents)
			{
				component.OnHighlight += delegate(Hand hand, Grabbable grabbable)
				{
					Debug.Log(hand.name + " HIGHLIGHT EVENT", this);
				};
			}
			if (showHighlightEvents)
			{
				component.OnStopHighlight += delegate(Hand hand, Grabbable grabbable)
				{
					Debug.Log(hand.name + " UNHIGHLIGHT EVENT", this);
				};
			}
		}

		private void OnDisable()
		{
			Hand component = GetComponent<Hand>();
			component.OnBeforeGrabbed -= delegate(Hand hand, Grabbable grabbable)
			{
				Debug.Log(hand.name + " BEFORE GRAB EVENT", this);
			};
			component.OnGrabbed -= delegate(Hand hand, Grabbable grabbable)
			{
				Debug.Log(hand.name + " GRAB EVENT", this);
			};
			component.OnReleased -= delegate(Hand hand, Grabbable grabbable)
			{
				Debug.Log(hand.name + " RELEASE EVENT", this);
			};
			component.OnGrabJointBreak -= delegate(Hand hand, Grabbable grabbable)
			{
				Debug.Log(hand.name + " CONNECTION BREAK EVENT", this);
			};
			if (showSqueezeEvents)
			{
				component.OnSqueezed -= delegate(Hand hand, Grabbable grabbable)
				{
					Debug.Log(hand.name + " SQUEEZE EVENT", this);
				};
			}
			if (showSqueezeEvents)
			{
				component.OnUnsqueezed -= delegate(Hand hand, Grabbable grabbable)
				{
					Debug.Log(hand.name + " UNSQUEEZE EVENT", this);
				};
			}
			if (showHighlightEvents)
			{
				component.OnHighlight -= delegate(Hand hand, Grabbable grabbable)
				{
					Debug.Log(hand.name + " HIGHLIGHT EVENT", this);
				};
			}
			if (showHighlightEvents)
			{
				component.OnStopHighlight -= delegate(Hand hand, Grabbable grabbable)
				{
					Debug.Log(hand.name + " UNHIGHLIGHT EVENT", this);
				};
			}
		}
	}
}
