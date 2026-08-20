using System;
using UnityEngine;

public class FlyCamera : MonoBehaviour
{
	public float XSensitivity = 2f;

	public float YSensitivity = 2f;

	public bool clampVerticalRotation = true;

	public float MinimumX = -90f;

	public float MaximumX = 90f;

	public bool lockCursor = true;

	private Quaternion m_CharacterTargetRot;

	private Quaternion m_CameraTargetRot;

	private bool m_cursorIsLocked = true;

	private Vector3 m_MoveDir = Vector3.zero;

	[SerializeField]
	private float m_WalkSpeed;

	private Vector2 m_Input;

	private Camera m_Camera;

	private Vector3 m_OriginalCameraPosition;

	private void Start()
	{
		m_CharacterTargetRot = base.transform.transform.localRotation;
		m_CameraTargetRot = Camera.main.transform.localRotation;
		m_Camera = Camera.main;
	}

	private Quaternion ClampRotationAroundXAxis(Quaternion q)
	{
		q.x /= q.w;
		q.y /= q.w;
		q.z /= q.w;
		q.w = 1f;
		float value = 114.59156f * Mathf.Atan(q.x);
		value = Mathf.Clamp(value, MinimumX, MaximumX);
		q.x = Mathf.Tan(MathF.PI / 360f * value);
		return q;
	}

	private void InternalLockUpdate()
	{
		if (Input.GetKeyUp(KeyCode.Escape))
		{
			m_cursorIsLocked = false;
		}
		else if (Input.GetMouseButtonUp(0))
		{
			m_cursorIsLocked = true;
		}
		if (m_cursorIsLocked)
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
		else if (!m_cursorIsLocked)
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
	}

	public void UpdateCursorLock()
	{
		if (lockCursor)
		{
			InternalLockUpdate();
		}
	}

	private void GetInput(out float speed)
	{
		float axis = Input.GetAxis("Horizontal");
		float axis2 = Input.GetAxis("Vertical");
		speed = m_WalkSpeed;
		m_Input = new Vector2(axis, axis2);
		if (m_Input.sqrMagnitude > 1f)
		{
			m_Input.Normalize();
		}
	}

	private void UpdateCameraPosition(float speed)
	{
		Vector3 localPosition = m_Camera.transform.localPosition;
		localPosition.y = m_Camera.transform.localPosition.y;
	}

	private void Update()
	{
		GetInput(out var speed);
		Vector3 vector = Camera.main.transform.forward * m_Input.y + base.transform.right * m_Input.x;
		m_MoveDir.x = vector.x * speed;
		m_MoveDir.y = vector.y * speed;
		m_MoveDir.z = vector.z * speed;
		base.transform.position += m_MoveDir * Time.fixedDeltaTime;
		UpdateCameraPosition(speed);
		LookRotation();
	}

	public void LookRotation()
	{
		float y = Input.GetAxis("Mouse X") * XSensitivity;
		float num = Input.GetAxis("Mouse Y") * YSensitivity;
		m_CharacterTargetRot *= Quaternion.Euler(0f, y, 0f);
		m_CameraTargetRot *= Quaternion.Euler(0f - num, 0f, 0f);
		if (clampVerticalRotation)
		{
			m_CameraTargetRot = ClampRotationAroundXAxis(m_CameraTargetRot);
		}
		base.transform.transform.localRotation = m_CharacterTargetRot;
		Camera.main.transform.localRotation = m_CameraTargetRot;
		UpdateCursorLock();
	}
}
