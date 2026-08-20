using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Autohand
{
	public class Teleporter : MonoBehaviour
	{
		[Header("Teleport")]
		[Tooltip("The object to teleport")]
		public GameObject teleportObject;

		[Tooltip("Can be left empty - Used for if there is a container that should be teleported in addition to the main teleport object")]
		public Transform[] additionalTeleports;

		[Header("Aim Settings")]
		[Tooltip("The Object to Shoot the Beam From")]
		public Transform aimer;

		[Tooltip("Layers You Can Teleport On")]
		public LayerMask layer;

		[Tooltip("The Maximum Slope You Can Teleport On")]
		public float maxSurfaceAngle = 45f;

		[Min(0f)]
		public float distanceMultiplyer = 1f;

		[Min(0f)]
		public float curveStrength = 1f;

		[Tooltip("Use Worldspace Must be True")]
		public LineRenderer line;

		[Tooltip("Maximum Length of The Teleport Line")]
		public int lineSegments = 50;

		[Header("Line Settings")]
		public Gradient canTeleportColor = new Gradient
		{
			colorKeys = new GradientColorKey[1]
			{
				new GradientColorKey
				{
					color = Color.green,
					time = 0f
				}
			}
		};

		public Gradient cantTeleportColor = new Gradient
		{
			colorKeys = new GradientColorKey[1]
			{
				new GradientColorKey
				{
					color = Color.red,
					time = 0f
				}
			}
		};

		[Tooltip("This gameobject will match the position of the teleport point when aiming")]
		public GameObject indicator;

		[Header("Unity Events")]
		public UnityEvent OnStartTeleport;

		public UnityEvent OnStopTeleport;

		public UnityEvent OnTeleport;

		private Vector3[] lineArr;

		private bool aiming;

		private bool hitting;

		private RaycastHit aimHit;

		private HandTeleportGuard[] teleportGuards;

		private AutoHandPlayer playerBody;

		private void Start()
		{
			playerBody = Object.FindObjectOfType<AutoHandPlayer>();
			if (playerBody != null && playerBody.transform.gameObject == teleportObject)
			{
				teleportObject = null;
			}
			lineArr = new Vector3[lineSegments];
			teleportGuards = Object.FindObjectsOfType<HandTeleportGuard>();
		}

		private void Update()
		{
			if (aiming)
			{
				CalculateTeleport();
			}
			else
			{
				line.positionCount = 0;
			}
			DrawIndicator();
		}

		private void CalculateTeleport()
		{
			line.colorGradient = cantTeleportColor;
			List<Vector3> list = new List<Vector3>();
			hitting = false;
			int i;
			for (i = 0; i < lineSegments; i++)
			{
				float num = (float)i / 60f;
				lineArr[i] = aimer.transform.position;
				lineArr[i] += base.transform.forward * num * distanceMultiplyer * 15f;
				lineArr[i].y += curveStrength * (num - Mathf.Pow(4.9f * num, 2f));
				list.Add(lineArr[i]);
				if (i != 0 && Physics.Raycast(lineArr[i - 1], lineArr[i] - lineArr[i - 1], out aimHit, Vector3.Distance(lineArr[i], lineArr[i - 1]), ~HandBase.GetHandsLayerMask(), QueryTriggerInteraction.Ignore))
				{
					if (Vector3.Angle(aimHit.normal, Vector3.up) <= maxSurfaceAngle && (int)layer == ((int)layer | (1 << aimHit.collider.gameObject.layer)))
					{
						line.colorGradient = canTeleportColor;
						list.Add(aimHit.point);
						hitting = true;
					}
					break;
				}
			}
			line.positionCount = i;
			line.SetPositions(lineArr);
		}

		private void DrawIndicator()
		{
			if (indicator != null)
			{
				if (hitting)
				{
					indicator.gameObject.SetActive(value: true);
					indicator.transform.position = aimHit.point;
					indicator.transform.up = aimHit.normal;
				}
				else
				{
					indicator.gameObject.SetActive(value: false);
				}
			}
		}

		public void StartTeleport()
		{
			aiming = true;
			OnStartTeleport?.Invoke();
		}

		public void CancelTeleport()
		{
			line.positionCount = 0;
			hitting = false;
			aiming = false;
			OnStopTeleport?.Invoke();
		}

		public void Teleport()
		{
			Queue<Vector3> queue = new Queue<Vector3>();
			HandTeleportGuard[] array = teleportGuards;
			foreach (HandTeleportGuard handTeleportGuard in array)
			{
				if (handTeleportGuard.gameObject.activeInHierarchy)
				{
					queue.Enqueue(handTeleportGuard.transform.position);
				}
			}
			if (hitting)
			{
				if (teleportObject != null)
				{
					Vector3 vector = aimHit.point - teleportObject.transform.position;
					teleportObject.transform.position = aimHit.point;
					Transform[] array2 = additionalTeleports;
					for (int i = 0; i < array2.Length; i++)
					{
						array2[i].position += vector;
					}
				}
				playerBody?.SetPosition(aimHit.point);
				OnTeleport?.Invoke();
				array = teleportGuards;
				foreach (HandTeleportGuard handTeleportGuard2 in array)
				{
					if (handTeleportGuard2.gameObject.activeInHierarchy)
					{
						handTeleportGuard2.TeleportProtection(queue.Dequeue(), handTeleportGuard2.transform.position);
					}
				}
			}
			CancelTeleport();
		}
	}
}
