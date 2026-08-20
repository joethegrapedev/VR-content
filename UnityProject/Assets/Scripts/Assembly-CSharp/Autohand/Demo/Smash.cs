using System;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

namespace Autohand.Demo
{
	public class Smash : MonoBehaviour
	{
		[Header("Smash Options")]
		[Tooltip("Required velocity magnitude from Smasher to smash")]
		public float smashForce = 1f;

		[Tooltip("Whether or not to destroy this object on smash")]
		public bool destroyOnSmash;

		[Tooltip("Whether or not to release this object on smash")]
		[HideIf("destroyOnSmash")]
		public bool releaseOnSmash;

		[Header("Particle Effect")]
		[Tooltip("Plays this effect on smash")]
		public ParticleSystem effect;

		[Tooltip("Whether or not to instantiates a new a particle system on smash")]
		public bool createNewEffect = true;

		[Tooltip("Whether or not to apply rigidbody velocity to particle velocity on smash")]
		public bool applyVelocityOnSmash = true;

		[Header("Sound Options")]
		public AudioClip smashSound;

		public float smashVolume = 1f;

		[Header("Event")]
		public UnityEvent OnSmash;

		public SmashEvent OnSmashEvent;

		internal Grabbable grabbable;

		public void Start()
		{
			GrabbableChild component;
			if (!(grabbable = GetComponent<Grabbable>()) && (bool)(component = GetComponent<GrabbableChild>()))
			{
				grabbable = component.grabParent;
			}
			OnSmashEvent = (SmashEvent)Delegate.Combine(OnSmashEvent, (SmashEvent)delegate
			{
				OnSmash?.Invoke();
			});
		}

		public void DelayedSmash(float delay)
		{
			Invoke("DoSmash", delay);
		}

		public void DoSmash()
		{
			DoSmash(null);
		}

		public void DoSmash(Smasher smash)
		{
			if ((bool)effect)
			{
				ParticleSystem particleSystem = ((!createNewEffect) ? effect : UnityEngine.Object.Instantiate(effect, grabbable.transform.position, grabbable.transform.rotation));
				particleSystem.transform.parent = null;
				particleSystem.Play();
				Rigidbody component;
				if (applyVelocityOnSmash && ((bool)(component = grabbable.body) || base.gameObject.CanGetComponent<Rigidbody>(out component)))
				{
					ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime = particleSystem.velocityOverLifetime;
					velocityOverLifetime.x = component.velocity.x;
					velocityOverLifetime.y = component.velocity.y;
					velocityOverLifetime.z = component.velocity.z;
				}
			}
			if ((bool)smashSound)
			{
				AudioSource.PlayClipAtPoint(smashSound, base.transform.position, smashVolume);
			}
			OnSmashEvent?.Invoke(smash, this);
			if ((destroyOnSmash || releaseOnSmash) && (bool)grabbable)
			{
				grabbable.ForceHandsRelease();
			}
			if (destroyOnSmash)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
	}
}
