using System;
using NaughtyAttributes;
using UnityEngine;

namespace Autohand
{
	public class HandTouchButton : MonoBehaviour
	{
		[HideIf("startUnpress")]
		public bool startPress;

		[HideIf("startPress")]
		public bool startUnpress;

		public HandTouchEvent touchEvent;

		public Transform button;

		public Vector3 pressOffset;

		public Color unpressColor = Color.white;

		public Color pressColor = Color.white;

		public bool toggle = true;

		[Space]
		public UnityHandEvent OnPressed;

		public UnityHandEvent OnUnpressed;

		private bool pressed;

		private void Start()
		{
			if (startPress)
			{
				PressButton(null);
			}
			else if (startUnpress)
			{
				ReleaseButton(null);
			}
		}

		private void OnEnable()
		{
			HandTouchEvent handTouchEvent = touchEvent;
			handTouchEvent.HandStartTouchEvent = (HandEvent)Delegate.Combine(handTouchEvent.HandStartTouchEvent, new HandEvent(OnTouch));
			HandTouchEvent handTouchEvent2 = touchEvent;
			handTouchEvent2.HandStopTouchEvent = (HandEvent)Delegate.Combine(handTouchEvent2.HandStopTouchEvent, new HandEvent(OnUntouch));
		}

		private void OnDisable()
		{
			HandTouchEvent handTouchEvent = touchEvent;
			handTouchEvent.HandStartTouchEvent = (HandEvent)Delegate.Remove(handTouchEvent.HandStartTouchEvent, new HandEvent(OnTouch));
			HandTouchEvent handTouchEvent2 = touchEvent;
			handTouchEvent2.HandStopTouchEvent = (HandEvent)Delegate.Remove(handTouchEvent2.HandStopTouchEvent, new HandEvent(OnUntouch));
		}

		private void OnTouch(Hand hand)
		{
			if (toggle)
			{
				if (!pressed)
				{
					PressButton(hand);
				}
				else if (pressed)
				{
					ReleaseButton(hand);
				}
			}
			else if (!pressed)
			{
				PressButton(hand);
			}
		}

		private void OnUntouch(Hand hand)
		{
			if (pressed && !toggle)
			{
				ReleaseButton(hand);
			}
		}

		private void PressButton(Hand hand)
		{
			if (!pressed)
			{
				button.localPosition += pressOffset;
			}
			pressed = true;
			OnPressed?.Invoke(hand);
			button.GetComponent<MeshRenderer>().material.color = pressColor;
		}

		private void ReleaseButton(Hand hand)
		{
			if (pressed)
			{
				button.localPosition -= pressOffset;
			}
			pressed = false;
			OnUnpressed?.Invoke(hand);
			button.GetComponent<MeshRenderer>().material.color = unpressColor;
		}
	}
}
