using System;
using System.Collections.Generic;
using UnityEngine;

namespace Autohand
{
	[HelpURL("https://www.notion.so/Touch-Events-1341b3e627dd443a99593ff7f0412aa6")]
	public class HandTouchEvent : MonoBehaviour
	{
		[Header("For Solid Collision")]
		[Tooltip("Whether or not first hand to enter should take ownership and be the only one to call events")]
		public bool oneHanded = true;

		public HandType handType;

		[Header("Events")]
		public UnityHandEvent HandStartTouch;

		public UnityHandEvent HandStopTouch;

		public HandEvent HandStartTouchEvent;

		public HandEvent HandStopTouchEvent;

		private List<Hand> hands;

		private void OnEnable()
		{
			hands = new List<Hand>();
			HandStartTouchEvent = (HandEvent)Delegate.Combine(HandStartTouchEvent, (HandEvent)delegate(Hand hand)
			{
				HandStartTouch?.Invoke(hand);
			});
			HandStopTouchEvent = (HandEvent)Delegate.Combine(HandStopTouchEvent, (HandEvent)delegate(Hand hand)
			{
				HandStopTouch?.Invoke(hand);
			});
		}

		private void OnDisable()
		{
			HandStartTouchEvent = (HandEvent)Delegate.Remove(HandStartTouchEvent, (HandEvent)delegate(Hand hand)
			{
				HandStartTouch?.Invoke(hand);
			});
			HandStopTouchEvent = (HandEvent)Delegate.Remove(HandStopTouchEvent, (HandEvent)delegate(Hand hand)
			{
				HandStopTouch?.Invoke(hand);
			});
		}

		public void Touch(Hand hand)
		{
			if (base.enabled && handType != HandType.none && (!hand.left || handType != HandType.right) && (hand.left || handType != HandType.left) && !hands.Contains(hand))
			{
				if (oneHanded && hands.Count == 0)
				{
					HandStartTouchEvent?.Invoke(hand);
				}
				else
				{
					HandStartTouchEvent?.Invoke(hand);
				}
				hands.Add(hand);
			}
		}

		public void Untouch(Hand hand)
		{
			if (base.enabled && handType != HandType.none && (!hand.left || handType != HandType.right) && (hand.left || handType != HandType.left) && hands.Contains(hand))
			{
				if (oneHanded && hands[0] == hand)
				{
					HandStopTouchEvent?.Invoke(hand);
				}
				else if (!oneHanded)
				{
					HandStopTouchEvent?.Invoke(hand);
				}
				hands.Remove(hand);
			}
		}
	}
}
