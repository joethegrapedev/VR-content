using System;
using System.Collections.Generic;
using UnityEngine;

namespace Autohand
{
	public class HandTriggerAreaEvents : MonoBehaviour
	{
		[Header("Trigger Events Settings")]
		[Tooltip("Whether or not first hand to enter should take ownership and be the only one to call events")]
		public bool oneHanded = true;

		public HandType handType;

		[Tooltip("Whether or not to call the release event if exiting while grab event activated")]
		public bool exitTriggerRelease = true;

		[Tooltip("Whether or not to call the release event if exiting while grab event activated")]
		public bool exitTriggerUnsqueeze = true;

		[Header("Events")]
		public UnityHandEvent HandEnter;

		public UnityHandEvent HandExit;

		public UnityHandEvent HandGrab;

		public UnityHandEvent HandRelease;

		public UnityHandEvent HandSqueeze;

		public UnityHandEvent HandUnsqueeze;

		public HandEvent HandEnterEvent;

		public HandEvent HandExitEvent;

		public HandEvent HandGrabEvent;

		public HandEvent HandReleaseEvent;

		public HandEvent HandSqueezeEvent;

		public HandEvent HandUnsqueezeEvent;

		private List<Hand> hands;

		private bool grabbing;

		private bool squeezing;

		protected virtual void OnEnable()
		{
			hands = new List<Hand>();
			HandEnterEvent = (HandEvent)Delegate.Combine(HandEnterEvent, (HandEvent)delegate(Hand hand)
			{
				HandEnter?.Invoke(hand);
			});
			HandExitEvent = (HandEvent)Delegate.Combine(HandExitEvent, (HandEvent)delegate(Hand hand)
			{
				HandExit?.Invoke(hand);
			});
			HandGrabEvent = (HandEvent)Delegate.Combine(HandGrabEvent, (HandEvent)delegate(Hand hand)
			{
				HandGrab?.Invoke(hand);
			});
			HandReleaseEvent = (HandEvent)Delegate.Combine(HandReleaseEvent, (HandEvent)delegate(Hand hand)
			{
				HandRelease?.Invoke(hand);
			});
			HandSqueezeEvent = (HandEvent)Delegate.Combine(HandSqueezeEvent, (HandEvent)delegate(Hand hand)
			{
				HandSqueeze?.Invoke(hand);
			});
			HandUnsqueezeEvent = (HandEvent)Delegate.Combine(HandUnsqueezeEvent, (HandEvent)delegate(Hand hand)
			{
				HandUnsqueeze?.Invoke(hand);
			});
		}

		protected virtual void OnDisable()
		{
			HandEnterEvent = (HandEvent)Delegate.Remove(HandEnterEvent, (HandEvent)delegate(Hand hand)
			{
				HandEnter?.Invoke(hand);
			});
			HandExitEvent = (HandEvent)Delegate.Remove(HandExitEvent, (HandEvent)delegate(Hand hand)
			{
				HandExit?.Invoke(hand);
			});
			HandGrabEvent = (HandEvent)Delegate.Remove(HandGrabEvent, (HandEvent)delegate(Hand hand)
			{
				HandGrab?.Invoke(hand);
			});
			HandReleaseEvent = (HandEvent)Delegate.Remove(HandReleaseEvent, (HandEvent)delegate(Hand hand)
			{
				HandRelease?.Invoke(hand);
			});
			HandSqueezeEvent = (HandEvent)Delegate.Remove(HandSqueezeEvent, (HandEvent)delegate(Hand hand)
			{
				HandSqueeze?.Invoke(hand);
			});
			HandUnsqueezeEvent = (HandEvent)Delegate.Remove(HandUnsqueezeEvent, (HandEvent)delegate(Hand hand)
			{
				HandUnsqueeze?.Invoke(hand);
			});
			for (int num = hands.Count - 1; num >= 0; num--)
			{
				hands[num].RemoveHandTriggerArea(this);
			}
		}

		protected virtual void Update()
		{
			foreach (Hand hand in hands)
			{
				if (!hand.enabled)
				{
					Exit(hand);
					Release(hand);
				}
			}
		}

		public virtual void Enter(Hand hand)
		{
			if (base.enabled && handType != HandType.none && (!hand.left || handType != HandType.right) && (hand.left || handType != HandType.left) && !hands.Contains(hand))
			{
				hands.Add(hand);
				if (oneHanded && hands.Count == 1)
				{
					HandEnterEvent?.Invoke(hand);
				}
				else
				{
					HandEnterEvent?.Invoke(hand);
				}
			}
		}

		public virtual void Exit(Hand hand)
		{
			if (!base.enabled || handType == HandType.none || (hand.left && handType == HandType.right) || (!hand.left && handType == HandType.left) || !hands.Contains(hand))
			{
				return;
			}
			if (oneHanded && hands[0] == hand)
			{
				HandExit?.Invoke(hand);
				if (grabbing && exitTriggerRelease)
				{
					HandReleaseEvent?.Invoke(hand);
					grabbing = false;
				}
				if (squeezing && exitTriggerUnsqueeze)
				{
					HandUnsqueezeEvent?.Invoke(hand);
					squeezing = false;
				}
				if (hands.Count > 1)
				{
					HandEnterEvent?.Invoke(hands[1]);
				}
			}
			else if (!oneHanded)
			{
				HandExitEvent?.Invoke(hand);
				if (grabbing && exitTriggerRelease)
				{
					HandReleaseEvent?.Invoke(hand);
					grabbing = false;
				}
				if (squeezing && exitTriggerUnsqueeze)
				{
					HandUnsqueezeEvent?.Invoke(hand);
					squeezing = false;
				}
			}
			hands.Remove(hand);
		}

		public virtual void Grab(Hand hand)
		{
			if (base.enabled && handType != HandType.none && (!hand.left || handType != HandType.right) && (hand.left || handType != HandType.left) && !grabbing)
			{
				if (oneHanded && hands[0] == hand)
				{
					HandGrabEvent?.Invoke(hand);
					grabbing = true;
				}
				else if (!oneHanded)
				{
					HandGrabEvent?.Invoke(hand);
					grabbing = true;
				}
			}
		}

		public virtual void Release(Hand hand)
		{
			if (base.enabled && handType != HandType.none && (!hand.left || handType != HandType.right) && (hand.left || handType != HandType.left) && grabbing)
			{
				if (oneHanded && hands[0] == hand)
				{
					HandReleaseEvent?.Invoke(hand);
					grabbing = false;
				}
				else if (!oneHanded)
				{
					HandReleaseEvent?.Invoke(hand);
					grabbing = false;
				}
			}
		}

		public virtual void Squeeze(Hand hand)
		{
			if (base.enabled && handType != HandType.none && (!hand.left || handType != HandType.right) && (hand.left || handType != HandType.left) && !squeezing)
			{
				if (oneHanded && hands[0] == hand)
				{
					HandSqueezeEvent?.Invoke(hand);
					squeezing = true;
				}
				else if (!oneHanded)
				{
					squeezing = true;
					HandSqueezeEvent?.Invoke(hand);
				}
			}
		}

		public virtual void Unsqueeze(Hand hand)
		{
			if (base.enabled && handType != HandType.none && (!hand.left || handType != HandType.right) && (hand.left || handType != HandType.left) && squeezing)
			{
				if (oneHanded && hands[0] == hand)
				{
					HandUnsqueezeEvent?.Invoke(hand);
					squeezing = false;
				}
				else if (!oneHanded)
				{
					squeezing = false;
					HandUnsqueezeEvent?.Invoke(hand);
				}
			}
		}
	}
}
