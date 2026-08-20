using System;
using UnityEngine;

namespace Autohand.Demo
{
	[RequireComponent(typeof(Grabbable))]
	public class GrabbableEventDebugger : MonoBehaviour
	{
		private void OnEnable()
		{
			Grabbable component = GetComponent<Grabbable>();
			component.OnBeforeGrabEvent = (HandGrabEvent)Delegate.Combine(component.OnBeforeGrabEvent, (HandGrabEvent)delegate(Hand hand, Grabbable grabbable)
			{
				Debug.Log(grabbable.name + " BEFORE GRAB EVENT");
			});
			component.OnGrabEvent = (HandGrabEvent)Delegate.Combine(component.OnGrabEvent, (HandGrabEvent)delegate(Hand hand, Grabbable grabbable)
			{
				Debug.Log(grabbable.name + " GRAB EVENT");
			});
			component.OnReleaseEvent = (HandGrabEvent)Delegate.Combine(component.OnReleaseEvent, (HandGrabEvent)delegate(Hand hand, Grabbable grabbable)
			{
				Debug.Log(grabbable.name + " RELEASE EVENT");
			});
			component.OnJointBreakEvent = (HandGrabEvent)Delegate.Combine(component.OnJointBreakEvent, (HandGrabEvent)delegate(Hand hand, Grabbable grabbable)
			{
				Debug.Log(grabbable.name + " JOINT BREAK EVENT");
			});
			component.OnSqueezeEvent = (HandGrabEvent)Delegate.Combine(component.OnSqueezeEvent, (HandGrabEvent)delegate(Hand hand, Grabbable grabbable)
			{
				Debug.Log(grabbable.name + " SQUEEZE EVENT");
			});
			component.OnUnsqueezeEvent = (HandGrabEvent)Delegate.Combine(component.OnUnsqueezeEvent, (HandGrabEvent)delegate(Hand hand, Grabbable grabbable)
			{
				Debug.Log(grabbable.name + " UNSQUEEZE EVENT");
			});
			component.OnHighlightEvent = (HandGrabEvent)Delegate.Combine(component.OnHighlightEvent, (HandGrabEvent)delegate(Hand hand, Grabbable grabbable)
			{
				Debug.Log(grabbable.name + " HIGHLIGHT EVENT");
			});
			component.OnUnhighlightEvent = (HandGrabEvent)Delegate.Combine(component.OnUnhighlightEvent, (HandGrabEvent)delegate(Hand hand, Grabbable grabbable)
			{
				Debug.Log(grabbable.name + " UNHIGHLIGHT EVENT");
			});
		}

		private void OnDisable()
		{
			Grabbable component = GetComponent<Grabbable>();
			component.OnBeforeGrabEvent = (HandGrabEvent)Delegate.Remove(component.OnBeforeGrabEvent, (HandGrabEvent)delegate(Hand hand, Grabbable grabbable)
			{
				Debug.Log(grabbable.name + " BEFORE GRAB EVENT");
			});
			component.OnGrabEvent = (HandGrabEvent)Delegate.Remove(component.OnGrabEvent, (HandGrabEvent)delegate(Hand hand, Grabbable grabbable)
			{
				Debug.Log(grabbable.name + " GRAB EVENT");
			});
			component.OnReleaseEvent = (HandGrabEvent)Delegate.Remove(component.OnReleaseEvent, (HandGrabEvent)delegate(Hand hand, Grabbable grabbable)
			{
				Debug.Log(grabbable.name + " RELEASE EVENT");
			});
			component.OnJointBreakEvent = (HandGrabEvent)Delegate.Remove(component.OnJointBreakEvent, (HandGrabEvent)delegate(Hand hand, Grabbable grabbable)
			{
				Debug.Log(grabbable.name + " JOINT BREAK EVENT");
			});
			component.OnSqueezeEvent = (HandGrabEvent)Delegate.Remove(component.OnSqueezeEvent, (HandGrabEvent)delegate(Hand hand, Grabbable grabbable)
			{
				Debug.Log(grabbable.name + " SQUEEZE EVENT");
			});
			component.OnUnsqueezeEvent = (HandGrabEvent)Delegate.Remove(component.OnUnsqueezeEvent, (HandGrabEvent)delegate(Hand hand, Grabbable grabbable)
			{
				Debug.Log(grabbable.name + " UNSQUEEZE EVENT");
			});
			component.OnHighlightEvent = (HandGrabEvent)Delegate.Remove(component.OnHighlightEvent, (HandGrabEvent)delegate(Hand hand, Grabbable grabbable)
			{
				Debug.Log(grabbable.name + " HIGHLIGHT EVENT");
			});
			component.OnUnhighlightEvent = (HandGrabEvent)Delegate.Remove(component.OnUnhighlightEvent, (HandGrabEvent)delegate(Hand hand, Grabbable grabbable)
			{
				Debug.Log(grabbable.name + " UNHIGHLIGHT EVENT");
			});
		}
	}
}
