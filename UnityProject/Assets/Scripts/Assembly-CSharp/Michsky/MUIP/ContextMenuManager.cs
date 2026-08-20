using UnityEngine;
using UnityEngine.InputSystem;

namespace Michsky.MUIP
{
	[RequireComponent(typeof(Animator))]
	public class ContextMenuManager : MonoBehaviour
	{
		public enum CameraSource
		{
			Main = 0,
			Custom = 1
		}

		public enum SubMenuBehaviour
		{
			Hover = 0,
			Click = 1
		}

		public Canvas mainCanvas;

		public Camera targetCamera;

		public GameObject contextContent;

		public Animator contextAnimator;

		public GameObject contextButton;

		public GameObject contextSeparator;

		public GameObject contextSubMenu;

		public bool autoSubMenuPosition = true;

		public SubMenuBehaviour subMenuBehaviour;

		public CameraSource cameraSource;

		[Range(-50f, 50f)]
		public int vBorderTop = -10;

		[Range(-50f, 50f)]
		public int vBorderBottom = 10;

		[Range(-50f, 50f)]
		public int hBorderLeft = 15;

		[Range(-50f, 50f)]
		public int hBorderRight = -15;

		private Vector2 uiPos;

		private Vector3 cursorPos;

		private Vector3 contentPos = new Vector3(0f, 0f, 0f);

		private Vector3 contextVelocity = Vector3.zero;

		private RectTransform contextRect;

		private RectTransform contentRect;

		[HideInInspector]
		public bool isOn;

		[HideInInspector]
		public bool bottomLeft;

		[HideInInspector]
		public bool bottomRight;

		[HideInInspector]
		public bool topLeft;

		[HideInInspector]
		public bool topRight;

		private void Awake()
		{
			if (mainCanvas == null)
			{
				mainCanvas = base.gameObject.GetComponentInParent<Canvas>();
			}
			if (contextAnimator == null)
			{
				contextAnimator = base.gameObject.GetComponent<Animator>();
			}
			if (cameraSource == CameraSource.Main)
			{
				targetCamera = Camera.main;
			}
			contextRect = base.gameObject.GetComponent<RectTransform>();
			contentRect = contextContent.GetComponent<RectTransform>();
			contentPos = new Vector3(vBorderTop, hBorderLeft, 0f);
			base.gameObject.transform.SetAsLastSibling();
		}

		public void CheckForBounds()
		{
			if (uiPos.x <= -100f)
			{
				contentPos = new Vector3(hBorderLeft, contentPos.y, 0f);
				contentRect.pivot = new Vector2(0f, contentRect.pivot.y);
				bottomLeft = true;
			}
			else
			{
				bottomLeft = false;
			}
			if (uiPos.x >= 100f)
			{
				contentPos = new Vector3(hBorderRight, contentPos.y, 0f);
				contentRect.pivot = new Vector2(1f, contentRect.pivot.y);
				bottomRight = true;
			}
			else
			{
				bottomRight = false;
			}
			if (uiPos.y <= -75f)
			{
				contentPos = new Vector3(contentPos.x, vBorderBottom, 0f);
				contentRect.pivot = new Vector2(contentRect.pivot.x, 0f);
				topLeft = true;
			}
			else
			{
				topLeft = false;
			}
			if (uiPos.y >= 75f)
			{
				contentPos = new Vector3(contentPos.x, vBorderTop, 0f);
				contentRect.pivot = new Vector2(contentRect.pivot.x, 1f);
				topRight = true;
			}
			else
			{
				topRight = false;
			}
		}

		public void SetContextMenuPosition()
		{
			cursorPos = Mouse.current.position.ReadValue();
			uiPos = contextRect.anchoredPosition;
			CheckForBounds();
			if (mainCanvas.renderMode == RenderMode.ScreenSpaceCamera || mainCanvas.renderMode == RenderMode.WorldSpace)
			{
				contextRect.position = targetCamera.ScreenToWorldPoint(cursorPos);
				contextRect.localPosition = new Vector3(contextRect.localPosition.x, contextRect.localPosition.y, 0f);
				contextContent.transform.localPosition = Vector3.SmoothDamp(contextContent.transform.localPosition, contentPos, ref contextVelocity, 0f);
			}
			else if (mainCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
			{
				contextRect.position = cursorPos;
				contextContent.transform.position = new Vector3(cursorPos.x + contentPos.x, cursorPos.y + contentPos.y, 0f);
			}
		}

		public void Open()
		{
			contextAnimator.Play("Menu In");
			isOn = true;
		}

		public void Close()
		{
			contextAnimator.Play("Menu Out");
			isOn = false;
		}

		public void OpenContextMenu()
		{
			Open();
		}

		public void CloseOnClick()
		{
			Close();
		}
	}
}
