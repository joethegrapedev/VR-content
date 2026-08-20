using UnityEngine;

namespace Autohand
{
	public class KeyboardHand : MonoBehaviour
	{
		public Hand hand;

		public float speed = 1f;

		public float flySpeed = 1f;

		public bool useLocal = true;

		private void Update()
		{
			float num = 0f;
			if (Input.GetKey(KeyCode.Space))
			{
				num = 1f;
			}
			if (Input.GetKey(KeyCode.LeftShift))
			{
				num = -1f;
			}
			if (Input.GetKey(KeyCode.E))
			{
				base.transform.Rotate(new Vector3(speed * 90f * Time.deltaTime, 0f, 0f));
			}
			if (Input.GetKey(KeyCode.Q))
			{
				base.transform.Rotate(new Vector3((0f - speed) * 90f * Time.deltaTime, 0f, 0f));
			}
			if (useLocal)
			{
				Vector3 vector = new Vector3(num * flySpeed, (0f - Input.GetAxis("Horizontal")) * speed, Input.GetAxis("Vertical") * speed);
				base.transform.position += base.transform.rotation * vector * Time.deltaTime;
			}
			else
			{
				Vector3 vector2 = new Vector3(Input.GetAxis("Horizontal") * speed, num * flySpeed, Input.GetAxis("Vertical") * speed);
				base.transform.position += vector2 * Time.deltaTime;
			}
			if (Input.GetKeyDown(KeyCode.Mouse0))
			{
				hand.Grab();
			}
			if (Input.GetKeyUp(KeyCode.Mouse0))
			{
				hand.Release();
			}
		}
	}
}
