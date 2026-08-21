using UnityEngine;

/// <summary>
/// Plays footsteps as the player moves, driven by distance travelled rather
/// than a timer so the cadence always matches the actual speed.
///
/// Teleport locomotion is detected as a single large jump and deliberately
/// does not produce a stride; it plays the teleport cue instead.
/// </summary>
public class FootstepPlayer : MonoBehaviour
{
	[Tooltip("Transform to track. Falls back to the Player tag, then the main camera.")]
	public Transform tracked;

	[Tooltip("Metres of travel between footsteps")]
	public float strideLength = 0.85f;

	[Tooltip("A single frame's movement above this is a teleport, not a step")]
	public float teleportThreshold = 1.2f;

	[Tooltip("Ignore drift below this speed so standing still stays silent")]
	public float minimumSpeed = 0.25f;

	[Range(0f, 1f)]
	public float volume = 0.5f;

	public string footstepPrefix = "SFX_Footstep_Concrete";

	public string teleportClip = "SFX_Teleport";

	private Vector3 lastPosition;

	private float accumulatedDistance;

	private bool hasPosition;

	private void OnEnable()
	{
		hasPosition = false;
		accumulatedDistance = 0f;
	}

	private void Update()
	{
		Transform target = ResolveTarget();
		if (target == null)
		{
			return;
		}

		Vector3 position = target.position;
		if (!hasPosition)
		{
			lastPosition = position;
			hasPosition = true;
			return;
		}

		// Horizontal only: crouching or head bob is not walking.
		Vector3 delta = position - lastPosition;
		delta.y = 0f;
		lastPosition = position;

		float distance = delta.magnitude;
		if (distance > teleportThreshold)
		{
			accumulatedDistance = 0f;
			SfxPlayer.PlayAt(teleportClip, position, volume);
			return;
		}

		if (Time.deltaTime <= 0f || distance / Time.deltaTime < minimumSpeed)
		{
			return;
		}

		accumulatedDistance += distance;
		if (accumulatedDistance >= strideLength)
		{
			accumulatedDistance -= strideLength;
			SfxPlayer.PlayVariantAt(footstepPrefix, position, volume);
		}
	}

	private Transform ResolveTarget()
	{
		if (tracked != null)
		{
			return tracked;
		}

		GameObject tagged = GameObject.FindGameObjectWithTag("Player");
		if (tagged != null)
		{
			tracked = tagged.transform;
			return tracked;
		}

		Camera main = Camera.main;
		if (main != null)
		{
			tracked = main.transform;
		}
		return tracked;
	}
}
