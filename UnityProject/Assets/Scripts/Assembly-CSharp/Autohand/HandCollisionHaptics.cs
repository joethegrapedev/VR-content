using System.Collections;
using UnityEngine;

namespace Autohand
{
	[RequireComponent(typeof(Hand))]
	public class HandCollisionHaptics : MonoBehaviour
	{
		[Tooltip("The layers that cause the sound to play")]
		public LayerMask collisionTriggers = -1;

		public float hapticAmp = 0.8f;

		public float velocityAmp = 0.5f;

		public float repeatDelay = 0.2f;

		public float maxDuration = 0.5f;

		[Tooltip("Source to play sound from")]
		public AnimationCurve velocityAmpCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		[Tooltip("Source to play sound from")]
		public AnimationCurve velocityDurationCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		private Hand hand;

		private Rigidbody body;

		private bool canPlay = true;

		private Coroutine playRoutine;

		private void Start()
		{
			body = GetComponent<Rigidbody>();
			hand = GetComponent<Hand>();
			StartCoroutine(HapticPlayBuffer(1f));
		}

		private void OnDisable()
		{
			if (playRoutine != null)
			{
				StopCoroutine(playRoutine);
			}
		}

		private void OnCollisionEnter(Collision collision)
		{
			if (canPlay && (int)collisionTriggers == ((int)collisionTriggers | (1 << collision.gameObject.layer)) && body != null && (collision.collider.attachedRigidbody == null || collision.collider.attachedRigidbody.mass > 1E-07f))
			{
				float magnitude = collision.relativeVelocity.magnitude;
				hand.PlayHapticVibration(Mathf.Clamp(velocityDurationCurve.Evaluate(magnitude), 0f, maxDuration), velocityAmpCurve.Evaluate(magnitude * velocityAmp) * hapticAmp);
				if (playRoutine != null)
				{
					StopCoroutine(playRoutine);
				}
				playRoutine = StartCoroutine(PlayBuffer());
			}
		}

		private IEnumerator PlayBuffer()
		{
			canPlay = false;
			yield return new WaitForSeconds(repeatDelay);
			canPlay = true;
			playRoutine = null;
		}

		private IEnumerator HapticPlayBuffer(float time)
		{
			canPlay = false;
			yield return new WaitForSeconds(time);
			canPlay = true;
			playRoutine = null;
		}
	}
}
