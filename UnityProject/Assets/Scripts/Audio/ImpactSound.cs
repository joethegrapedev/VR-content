using UnityEngine;

/// <summary>
/// Plays a material-appropriate impact when a rigidbody hits something.
///
/// The clip is chosen from the object's name, and volume follows impact
/// speed, so a nudge is quiet and a drop is loud.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class ImpactSound : MonoBehaviour
{
	private const float MinimumSpeed = 0.7f;

	private const float LoudSpeed = 5f;

	private const float RepeatDelay = 0.12f;

	private const string WoodClip = "SFX_Wood_Impact";

	private const string MetalClip = "SFX_Metal_Impact";

	private const string CardboardClip = "SFX_Cardboard_Impact";

	private static readonly string[] WoodKeywords =
		{ "pallet", "wood", "crate", "plank", "timber" };

	private static readonly string[] MetalKeywords =
		{ "metal", "steel", "shelf", "rack", "pipe", "drum", "barrel", "trolley", "cart" };

	[Range(0f, 1f)]
	public float volumeScale = 0.7f;

	private string clipName;

	private float nextAllowedTime;

	private void Awake()
	{
		clipName = ResolveClip(gameObject.name);
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (Time.time < nextAllowedTime)
		{
			return;
		}

		float speed = collision.relativeVelocity.magnitude;
		if (speed < MinimumSpeed)
		{
			return;
		}

		nextAllowedTime = Time.time + RepeatDelay;
		float loudness = Mathf.Clamp01((speed - MinimumSpeed) / (LoudSpeed - MinimumSpeed));
		Vector3 point = (collision.contactCount > 0)
			? collision.GetContact(0).point
			: transform.position;
		SfxPlayer.PlayAt(clipName, point, loudness * volumeScale);
	}

	/// <summary>Pick a material from the object's name, defaulting to cardboard.</summary>
	public static string ResolveClip(string objectName)
	{
		string lowered = objectName.ToLowerInvariant();
		if (ContainsAny(lowered, MetalKeywords))
		{
			return MetalClip;
		}
		if (ContainsAny(lowered, WoodKeywords))
		{
			return WoodClip;
		}
		return CardboardClip;
	}

	private static bool ContainsAny(string text, string[] keywords)
	{
		for (int i = 0; i < keywords.Length; i++)
		{
			if (text.Contains(keywords[i]))
			{
				return true;
			}
		}
		return false;
	}
}
