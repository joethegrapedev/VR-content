using UnityEngine;
using UnityEngine.EventSystems;

namespace Autohand
{
	public class HandCanvasPointer : MonoBehaviour
	{
		[Header("References")]
		public GameObject hitPointMarker;

		private LineRenderer lineRenderer;

		[Header("Ray settings")]
		public float raycastLength = 8f;

		public bool autoShowTarget = true;

		public LayerMask UILayer;

		[Header("Events")]
		public UnityCanvasPointerEvent StartSelect;

		public UnityCanvasPointerEvent StopSelect;

		public UnityCanvasPointerEvent StartPoint;

		public UnityCanvasPointerEvent StopPoint;

		public GameObject _currTarget;

		private bool hover;

		private AutoInputModule inputModule;

		private float lineSegements = 10f;

		private static Camera cam;

		private int pointerIndex;

		public GameObject currTarget => _currTarget;

		public RaycastHit lastHit { get; private set; }

		public static Camera UICamera
		{
			get
			{
				if (cam == null)
				{
					cam = new GameObject("Camera Canvas Pointer (I AM CREATED AT RUNTIME FOR UI CANVAS INTERACTION, I AM NOT RENDERING ANYTHING, I AM NOT CREATING ADDITIONAL OVERHEAD)").AddComponent<Camera>();
					cam.clearFlags = CameraClearFlags.Nothing;
					cam.stereoTargetEye = StereoTargetEyeMask.None;
					cam.orthographic = true;
					cam.orthographicSize = 0.001f;
					cam.cullingMask = 0;
					cam.nearClipPlane = 0.01f;
					cam.depth = 0f;
					cam.allowHDR = false;
					cam.enabled = false;
					cam.fieldOfView = 1E-05f;
					cam.transform.parent = AutoHandExtensions.transformParent;
					Canvas[] array = Object.FindObjectsOfType<Canvas>(includeInactive: true);
					foreach (Canvas canvas in array)
					{
						if (canvas.renderMode == RenderMode.WorldSpace)
						{
							canvas.worldCamera = cam;
						}
					}
				}
				return cam;
			}
		}

		private void OnEnable()
		{
			lineRenderer.positionCount = (int)lineSegements;
			if (inputModule.Instance != null)
			{
				pointerIndex = inputModule.Instance.AddPointer(this);
			}
		}

		private void OnDisable()
		{
			if ((bool)inputModule)
			{
				inputModule.Instance?.RemovePointer(this);
			}
		}

		public void SetIndex(int index)
		{
			pointerIndex = index;
		}

		internal void Preprocess()
		{
			UICamera.transform.position = base.transform.position;
			UICamera.transform.forward = base.transform.forward;
		}

		public void Press()
		{
			if ((bool)inputModule)
			{
				inputModule.ProcessPress(pointerIndex);
			}
			if (!autoShowTarget && hover)
			{
				ShowRay(show: true);
			}
			if (lastHit.collider != null)
			{
				StartSelect?.Invoke(lastHit.point, lastHit.transform.gameObject);
				return;
			}
			PointerEventData data = inputModule.GetData(pointerIndex);
			float num = ((data.pointerCurrentRaycast.distance == 0f) ? raycastLength : data.pointerCurrentRaycast.distance);
			StartSelect?.Invoke(base.transform.position + base.transform.forward * num, null);
		}

		public void Release()
		{
			if ((bool)inputModule)
			{
				inputModule.ProcessRelease(pointerIndex);
			}
			if (lastHit.collider != null)
			{
				StopSelect?.Invoke(lastHit.point, lastHit.transform.gameObject);
				return;
			}
			PointerEventData data = inputModule.GetData(pointerIndex);
			float num = ((data.pointerCurrentRaycast.distance == 0f) ? raycastLength : data.pointerCurrentRaycast.distance);
			StopSelect?.Invoke(base.transform.position + base.transform.forward * num, null);
		}

		private void Awake()
		{
			if (lineRenderer == null)
			{
				base.gameObject.CanGetComponent<LineRenderer>(out lineRenderer);
			}
			if (!(inputModule == null))
			{
				return;
			}
			if (base.gameObject.CanGetComponent<AutoInputModule>(out var component))
			{
				inputModule = component;
			}
			else if (!(inputModule = Object.FindObjectOfType<AutoInputModule>()))
			{
				EventSystem eventSystem = Object.FindObjectOfType<EventSystem>();
				if (eventSystem == null)
				{
					eventSystem = new GameObject().AddComponent<EventSystem>();
					eventSystem.name = "UI Input Event System";
				}
				inputModule = eventSystem.gameObject.AddComponent<AutoInputModule>();
				inputModule.transform.parent = AutoHandExtensions.transformParent;
			}
		}

		private void Update()
		{
			UpdateLine();
		}

		private void UpdateLine()
		{
			PointerEventData data = inputModule.GetData(pointerIndex);
			float num = ((data.pointerCurrentRaycast.distance == 0f) ? raycastLength : data.pointerCurrentRaycast.distance);
			if (num > 0f)
			{
				_currTarget = data.pointerCurrentRaycast.gameObject;
			}
			else
			{
				_currTarget = null;
			}
			if (data.pointerCurrentRaycast.distance != 0f && !hover)
			{
				lastHit = CreateRaycast(num);
				Vector3 arg = base.transform.position + base.transform.forward * num;
				if ((bool)lastHit.collider)
				{
					arg = lastHit.point;
				}
				if (lastHit.collider != null)
				{
					StartPoint?.Invoke(lastHit.point, lastHit.transform.gameObject);
				}
				else
				{
					StartPoint?.Invoke(arg, null);
				}
				if (autoShowTarget)
				{
					ShowRay(show: true);
				}
				hover = true;
			}
			else if (data.pointerCurrentRaycast.distance == 0f && hover)
			{
				lastHit = CreateRaycast(num);
				Vector3 arg2 = base.transform.position + base.transform.forward * num;
				if ((bool)lastHit.collider)
				{
					arg2 = lastHit.point;
				}
				if (lastHit.collider != null)
				{
					StopPoint?.Invoke(lastHit.point, lastHit.transform.gameObject);
				}
				else
				{
					StopPoint?.Invoke(arg2, null);
				}
				ShowRay(show: false);
				hover = false;
			}
			if (hover)
			{
				lastHit = CreateRaycast(num);
				Vector3 vector = base.transform.position + base.transform.forward * num;
				if ((bool)lastHit.collider)
				{
					vector = lastHit.point;
				}
				hitPointMarker.transform.position = vector;
				hitPointMarker.transform.forward = data.pointerCurrentRaycast.worldNormal;
				if ((bool)lastHit.collider)
				{
					hitPointMarker.transform.forward = lastHit.collider.transform.forward;
					hitPointMarker.transform.position = vector + hitPointMarker.transform.forward * 0.002f;
				}
				for (int i = 0; (float)i < lineSegements; i++)
				{
					lineRenderer.SetPosition(i, Vector3.Lerp(base.transform.position, vector, (float)i / lineSegements));
				}
			}
		}

		private RaycastHit CreateRaycast(float dist)
		{
			Physics.Raycast(new Ray(base.transform.position, base.transform.forward), out var hitInfo, dist, UILayer);
			return hitInfo;
		}

		private void ShowRay(bool show)
		{
			hitPointMarker.SetActive(show);
			lineRenderer.enabled = show;
		}
	}
}
