using System;
using UnityEngine;

namespace Autohand.Demo
{
	public class HandTouchEventDebugger : MonoBehaviour
	{
		public HandTouchEvent touchEvent;

		private void OnEnable()
		{
			HandTouchEvent handTouchEvent = touchEvent;
			handTouchEvent.HandStartTouchEvent = (HandEvent)Delegate.Combine(handTouchEvent.HandStartTouchEvent, new HandEvent(StartTouch));
			HandTouchEvent handTouchEvent2 = touchEvent;
			handTouchEvent2.HandStopTouchEvent = (HandEvent)Delegate.Combine(handTouchEvent2.HandStopTouchEvent, new HandEvent(StopTouch));
		}

		private void OnDisable()
		{
			HandTouchEvent handTouchEvent = touchEvent;
			handTouchEvent.HandStartTouchEvent = (HandEvent)Delegate.Remove(handTouchEvent.HandStartTouchEvent, new HandEvent(StartTouch));
			HandTouchEvent handTouchEvent2 = touchEvent;
			handTouchEvent2.HandStopTouchEvent = (HandEvent)Delegate.Remove(handTouchEvent2.HandStopTouchEvent, new HandEvent(StopTouch));
		}

		private void StartTouch(Hand hand)
		{
			Debug.Log("Start Touch: " + hand.name);
		}

		private void StopTouch(Hand hand)
		{
			Debug.Log("Stop Touch: " + hand.name);
		}
	}
}
