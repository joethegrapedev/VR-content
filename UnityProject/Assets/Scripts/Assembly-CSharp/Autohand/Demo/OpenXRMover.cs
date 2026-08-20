using UnityEngine;
using UnityEngine.InputSystem;

namespace Autohand.Demo
{
	public class OpenXRMover : MonoBehaviour
	{
		[Header("Input Actions")]
		public InputActionProperty moveAction;

		public InputActionProperty turnAction;

		[Header("Body")]
		public GameObject cam;

		private CharacterController controller;

		[Header("Settings")]
		public bool snapTurning;

		public float turnAngle;

		public float heightStep;

		public float minHeight;

		public float maxHeight;

		public float speed = 5f;

		public float gravity = 1f;

		private float currentGravity;

		private bool turningReset = true;

		private bool heightReset = true;

		private void Start()
		{
			controller = GetComponent<CharacterController>();
			base.gameObject.layer = LayerMask.NameToLayer("HandPlayer");
			moveAction.action.Enable();
			moveAction.action.performed += Move;
			turnAction.action.Enable();
			turnAction.action.performed += TurnAndHeight;
		}

		private void Move(InputAction.CallbackContext move)
		{
			Vector3 euler = new Vector3(0f, cam.transform.eulerAngles.y, 0f);
			Vector2 vector = move.ReadValue<Vector2>();
			if (Mathf.Abs(vector.x) < 0.1f)
			{
				vector.x = 0f;
			}
			if (Mathf.Abs(vector.y) < 0.1f)
			{
				vector.y = 0f;
			}
			Vector3 vector2 = new Vector3(vector.x, 0f, vector.y);
			vector2 = Quaternion.Euler(euler) * vector2;
			if (controller.isGrounded)
			{
				currentGravity = 0f;
			}
			else
			{
				currentGravity = Physics.gravity.y * gravity;
			}
			controller.Move(new Vector3(vector2.x * speed, currentGravity, vector2.z * speed) * Time.deltaTime);
		}

		private void TurnAndHeight(InputAction.CallbackContext turn)
		{
			Vector2 vector = turn.ReadValue<Vector2>();
			if (snapTurning)
			{
				if (vector.x > 0.7f && turningReset)
				{
					base.transform.rotation *= Quaternion.Euler(0f, turnAngle, 0f);
					turningReset = false;
				}
				else if (vector.x < -0.7f && turningReset)
				{
					base.transform.rotation *= Quaternion.Euler(0f, 0f - turnAngle, 0f);
					turningReset = false;
				}
				else if (vector.y > 0.7f && heightReset)
				{
					if (base.transform.position.y >= maxHeight)
					{
						base.transform.position = new Vector3(base.transform.position.x, maxHeight, base.transform.position.z);
						SetControllerHeight(maxHeight);
					}
					else
					{
						base.transform.position += new Vector3(0f, heightStep, 0f);
						AddControllerHeight(heightStep);
					}
					heightReset = false;
				}
				else if (vector.y < -0.7f && heightReset)
				{
					if (base.transform.position.y <= minHeight)
					{
						SetControllerHeight(maxHeight);
						base.transform.position = new Vector3(base.transform.position.x, minHeight, base.transform.position.z);
					}
					else
					{
						AddControllerHeight(0f - heightStep);
						base.transform.position += new Vector3(0f, 0f - heightStep, 0f);
					}
					heightReset = false;
				}
				if (Mathf.Abs(vector.x) < 0.4f)
				{
					turningReset = true;
				}
				if (Mathf.Abs(vector.y) < 0.4f)
				{
					heightReset = true;
				}
			}
			else
			{
				base.transform.rotation *= Quaternion.Euler(0f, Time.deltaTime * turnAngle * vector.x, 0f);
				base.transform.position += new Vector3(0f, Time.deltaTime * heightStep * vector.y, 0f);
				AddControllerHeight(Time.deltaTime * heightStep * vector.y);
				if (base.transform.position.y <= minHeight)
				{
					base.transform.position = new Vector3(base.transform.position.x, minHeight, base.transform.position.z);
				}
				else if (base.transform.position.y >= maxHeight)
				{
					base.transform.position = new Vector3(base.transform.position.x, maxHeight, base.transform.position.z);
				}
			}
		}

		private void AddControllerHeight(float height)
		{
			controller.height += height;
			controller.center = new Vector3(0f, controller.height / 2f, 0f);
		}

		private void SetControllerHeight(float height)
		{
			controller.height = height;
			controller.center = new Vector3(0f, height / 2f, 0f);
		}
	}
}
