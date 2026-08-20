using Autohand;
using UnityEngine;

public class HandEventTemplate : MonoBehaviour
{
	public Hand hand;

	private void OnEnable()
	{
		hand.OnBeforeGrabbed += OnBeforeGrabbed;
		hand.OnGrabbed += OnGrabbed;
		hand.OnBeforeReleased += OnBeforeReleased;
		hand.OnReleased += OnReleased;
		hand.OnForcedRelease += OnForcedRelease;
		hand.OnGrabJointBreak += OnGrabJointBreak;
		hand.OnHandCollisionStart += OnHandCollisionStart;
		hand.OnHandCollisionStop += OnHandCollisionStop;
		hand.OnHandTriggerStart += OnHandTriggerStart;
		hand.OnHandTriggerStop += OnHandTriggerStop;
		hand.OnHighlight += OnHighlight;
		hand.OnStopHighlight += OnStopHighlight;
		hand.OnSqueezed += OnSqueezed;
		hand.OnUnsqueezed += OnUnsqueezed;
		hand.OnTriggerGrab += OnTriggerGrab;
		hand.OnTriggerRelease += OnTriggerRelease;
	}

	private void OnDisable()
	{
		hand.OnBeforeGrabbed -= OnBeforeGrabbed;
		hand.OnGrabbed -= OnGrabbed;
		hand.OnBeforeReleased -= OnBeforeReleased;
		hand.OnReleased -= OnReleased;
		hand.OnForcedRelease -= OnForcedRelease;
		hand.OnGrabJointBreak -= OnGrabJointBreak;
		hand.OnHighlight -= OnHighlight;
		hand.OnStopHighlight -= OnStopHighlight;
		hand.OnSqueezed -= OnSqueezed;
		hand.OnUnsqueezed -= OnUnsqueezed;
		hand.OnTriggerGrab -= OnTriggerGrab;
		hand.OnTriggerRelease -= OnTriggerRelease;
		hand.OnHandCollisionStart -= OnHandCollisionStart;
		hand.OnHandCollisionStop -= OnHandCollisionStop;
		hand.OnHandTriggerStart -= OnHandTriggerStart;
		hand.OnHandTriggerStop -= OnHandTriggerStop;
	}

	private void OnBeforeGrabbed(Hand hand, Grabbable grab)
	{
	}

	private void OnGrabbed(Hand hand, Grabbable grab)
	{
	}

	private void OnBeforeReleased(Hand hand, Grabbable grab)
	{
	}

	private void OnReleased(Hand hand, Grabbable grab)
	{
	}

	private void OnForcedRelease(Hand hand, Grabbable grab)
	{
	}

	private void OnGrabJointBreak(Hand hand, Grabbable grab)
	{
	}

	private void OnHighlight(Hand hand, Grabbable grab)
	{
	}

	private void OnStopHighlight(Hand hand, Grabbable grab)
	{
	}

	private void OnSqueezed(Hand hand, Grabbable grab)
	{
	}

	private void OnUnsqueezed(Hand hand, Grabbable grab)
	{
	}

	private void OnTriggerGrab(Hand hand, Grabbable grab)
	{
	}

	private void OnTriggerRelease(Hand hand, Grabbable grab)
	{
	}

	private void OnHandCollisionStart(Hand hand, GameObject other)
	{
	}

	private void OnHandCollisionStop(Hand hand, GameObject other)
	{
	}

	private void OnHandTriggerStart(Hand hand, GameObject other)
	{
	}

	private void OnHandTriggerStop(Hand hand, GameObject other)
	{
	}
}
