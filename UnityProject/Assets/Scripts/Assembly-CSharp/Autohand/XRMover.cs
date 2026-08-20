using Autohand.Demo;
using UnityEngine;

namespace Autohand
{
	public class XRMover : MonoBehaviour
	{
		[Header("TEMP DEMO SCRIPT - Advanced script coming soon")]
		[Header("Controllers")]
		public XRHandControllerLink moverController;

		public XRHandControllerLink turningController;

		public Common2DAxis moverAxis;

		[Header("Body")]
		public GameObject cam;

		private CharacterController controller;

		private CapsuleCollider collisionCapsule;

		[Header("Settings")]
		public bool snapTurning;

		public float turnAngle;

		public float speed = 5f;

		public float gravity = 1f;

		private float currentGravity;

		private bool axisReset = true;

		private Vector3 moveAxis;

		private Vector2 turningAxis;

		public void LateUpdate()
		{
			turningAxis = turningController.GetAxis2D(moverAxis);
			moveAxis = moverController.GetAxis2D(moverAxis);
			Move(moveAxis.x, moveAxis.z, moveAxis.y);
			Turning();
		}

		private void Awake()
		{
			base.gameObject.layer = LayerMask.NameToLayer("HandPlayer");
			controller = GetComponent<CharacterController>();
		}

		public void Move(float x, float y, float z)
		{
			Vector3 vector = new Vector3(x, y, z);
			vector = Quaternion.Euler(new Vector3(0f, cam.transform.eulerAngles.y, 0f)) * vector;
			currentGravity = Physics.gravity.y * gravity;
			if (controller.isGrounded)
			{
				currentGravity = 0f;
			}
			controller.Move(new Vector3(vector.x * speed, vector.y * speed + currentGravity, vector.z * speed) * Time.deltaTime);
		}

		private void Turning()
		{
			if (snapTurning)
			{
				if (turningAxis.x > 0.7f && axisReset)
				{
					base.transform.rotation *= Quaternion.Euler(0f, turnAngle, 0f);
					axisReset = false;
				}
				else if (turningAxis.x < -0.7f && axisReset)
				{
					base.transform.rotation *= Quaternion.Euler(0f, 0f - turnAngle, 0f);
					axisReset = false;
				}
				if (Mathf.Abs(turningAxis.x) < 0.4f)
				{
					axisReset = true;
				}
			}
			else
			{
				base.transform.rotation *= Quaternion.Euler(0f, Time.deltaTime * turnAngle * turningAxis.x, 0f);
			}
		}
	}
}
