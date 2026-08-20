using UnityEngine;

namespace Autohand.Demo
{
	public class Pistol : MonoBehaviour
	{
		public Rigidbody body;

		public Transform barrelTip;

		public float hitPower = 1f;

		public float recoilPower = 1f;

		public float range = 100f;

		public LayerMask layer;

		public AudioClip shootSound;

		public float shootVolume = 1f;

		private void Start()
		{
			if (body == null && GetComponent<Rigidbody>() != null)
			{
				body = GetComponent<Rigidbody>();
			}
		}

		public void Shoot()
		{
			if ((bool)shootSound)
			{
				AudioSource.PlayClipAtPoint(shootSound, base.transform.position, shootVolume);
			}
			if (Physics.Raycast(barrelTip.position, barrelTip.forward, out var hitInfo, range, layer))
			{
				Rigidbody component = hitInfo.transform.GetComponent<Rigidbody>();
				if (component != null)
				{
					Debug.DrawRay(barrelTip.position, hitInfo.point - barrelTip.position, Color.green, 5f);
					component.GetComponent<Smash>()?.DoSmash();
					component.AddForceAtPosition((hitInfo.point - barrelTip.position).normalized * hitPower * 10f, hitInfo.point, ForceMode.Impulse);
				}
			}
			else
			{
				Debug.DrawRay(barrelTip.position, barrelTip.forward * range, Color.red, 1f);
			}
			body.AddForce(barrelTip.transform.up * recoilPower * 5f, ForceMode.Impulse);
		}
	}
}
