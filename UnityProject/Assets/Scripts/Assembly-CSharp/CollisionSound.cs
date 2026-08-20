using System.Collections;
using UnityEngine;

public class CollisionSound : MonoBehaviour
{
	[Tooltip("The layers that cause the sound to play")]
	public LayerMask collisionTriggers = -1;

	[Tooltip("Source to play sound from")]
	public AudioSource source;

	[Tooltip("Source to play sound from")]
	public AudioClip clip;

	[Space]
	[Tooltip("Source to play sound from")]
	public AnimationCurve velocityVolumeCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public float volumeAmp = 0.8f;

	public float velocityAmp = 0.5f;

	public float soundRepeatDelay = 0.2f;

	private Rigidbody body;

	private bool canPlaySound = true;

	private Coroutine playSoundRoutine;

	private void Start()
	{
		body = GetComponent<Rigidbody>();
		StartCoroutine(SoundPlayBuffer(1f));
	}

	private void OnDisable()
	{
		if (playSoundRoutine != null)
		{
			StopCoroutine(playSoundRoutine);
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (!(body == null) && canPlaySound && (int)collisionTriggers == ((int)collisionTriggers | (1 << collision.gameObject.layer)) && source != null && source.enabled && (collision.collider.attachedRigidbody == null || collision.collider.attachedRigidbody.mass > 1E-07f))
		{
			if (clip != null || source.clip != null)
			{
				source.PlayOneShot((clip == null) ? source.clip : clip, velocityVolumeCurve.Evaluate(collision.relativeVelocity.magnitude * velocityAmp) * volumeAmp);
			}
			if (playSoundRoutine != null)
			{
				StopCoroutine(playSoundRoutine);
			}
			playSoundRoutine = StartCoroutine(SoundPlayBuffer());
		}
	}

	private IEnumerator SoundPlayBuffer()
	{
		canPlaySound = false;
		yield return new WaitForSeconds(soundRepeatDelay);
		canPlaySound = true;
		playSoundRoutine = null;
	}

	private IEnumerator SoundPlayBuffer(float time)
	{
		canPlaySound = false;
		yield return new WaitForSeconds(time);
		canPlaySound = true;
		playSoundRoutine = null;
	}
}
