using UnityEngine;

namespace Autohand.Demo
{
	public class Spinner : MonoBehaviour
	{
		public Vector3 rotationSpeed;

		private void FixedUpdate()
		{
			base.transform.Rotate(rotationSpeed * Time.fixedDeltaTime / 2f);
		}

		private void Update()
		{
			base.transform.Rotate(rotationSpeed * Time.deltaTime / 2f);
		}
	}
}
