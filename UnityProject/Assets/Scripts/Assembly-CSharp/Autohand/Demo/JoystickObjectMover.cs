using UnityEngine;

namespace Autohand.Demo
{
	public class JoystickObjectMover : PhysicsGadgetJoystick
	{
		public Transform move;

		public float speed = 2f;

		private void Update()
		{
			Vector2 vector = GetValue();
			Vector3 vector2 = new Vector3(vector.x * Time.deltaTime * speed, 0f, vector.y * Time.deltaTime * speed);
			move.transform.localPosition += vector2;
		}
	}
}
