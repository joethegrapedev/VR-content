using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Michsky.MUIP
{
	[ExecuteInEditMode]
	public class ButtonManager : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, ISubmitHandler
	{
		public enum AnimationSolution
		{
			Custom = 0,
			ScriptBased = 1
		}

		public enum RippleUpdateMode
		{
			Normal = 0,
			UnscaledTime = 1
		}

		[Serializable]
		public class Padding
		{
			public int left = 20;

			public int right = 20;

			public int top = 5;

			public int bottom = 5;
		}

		public Sprite buttonIcon;

		public string buttonText = "Button";

		[Range(0.1f, 10f)]
		public float iconScale = 1f;

		[Range(10f, 200f)]
		public float textSize = 24f;

		public bool autoFitContent = true;

		public Padding padding;

		[Range(0f, 100f)]
		public int spacing = 15;

		public HorizontalLayoutGroup disabledLayout;

		public HorizontalLayoutGroup normalLayout;

		public HorizontalLayoutGroup highlightedLayout;

		public HorizontalLayoutGroup mainLayout;

		public ContentSizeFitter mainFitter;

		public ContentSizeFitter targetFitter;

		public RectTransform targetRect;

		public CanvasGroup normalCG;

		public CanvasGroup highlightCG;

		public CanvasGroup disabledCG;

		public TextMeshProUGUI normalText;

		public TextMeshProUGUI highlightedText;

		public TextMeshProUGUI disabledText;

		public Image normalImage;

		public Image highlightImage;

		public Image disabledImage;

		public AudioSource soundSource;

		public GameObject rippleParent;

		public bool isInteractable = true;

		public bool enableIcon;

		public bool enableText = true;

		public bool useCustomContent;

		public bool useCustomIconSize;

		public bool useCustomTextSize;

		public bool checkForDoubleClick = true;

		public bool enableButtonSounds;

		public bool useHoverSound = true;

		public bool useClickSound = true;

		public AudioClip hoverSound;

		public AudioClip clickSound;

		public bool useUINavigation;

		public Navigation.Mode navigationMode = Navigation.Mode.Automatic;

		public GameObject selectOnUp;

		public GameObject selectOnDown;

		public GameObject selectOnLeft;

		public GameObject selectOnRight;

		public bool wrapAround;

		public bool useRipple = true;

		[Range(0.1f, 1f)]
		public float doubleClickPeriod = 0.25f;

		[Range(0.25f, 15f)]
		public float fadingMultiplier = 8f;

		public AnimationSolution animationSolution = AnimationSolution.ScriptBased;

		public UnityEvent onClick = new UnityEvent();

		public UnityEvent onDoubleClick = new UnityEvent();

		public UnityEvent onHover = new UnityEvent();

		public UnityEvent onLeave = new UnityEvent();

		public RippleUpdateMode rippleUpdateMode = RippleUpdateMode.UnscaledTime;

		public Sprite rippleShape;

		[Range(0.1f, 5f)]
		public float speed = 1f;

		[Range(0.5f, 25f)]
		public float maxSize = 4f;

		public Color startColor = new Color(1f, 1f, 1f, 0.2f);

		public Color transitionColor = new Color(1f, 1f, 1f, 0f);

		public bool renderOnTop;

		public bool centered;

		private Button targetButton;

		private bool isPointerOn;

		private bool waitingForDoubleClickInput;

		private void OnEnable()
		{
			UpdateUI();
		}

		private void OnDisable()
		{
			if (isInteractable)
			{
				if (disabledCG != null)
				{
					disabledCG.alpha = 0f;
				}
				if (normalCG != null)
				{
					normalCG.alpha = 1f;
				}
				if (highlightCG != null)
				{
					highlightCG.alpha = 0f;
				}
			}
		}

		private void Awake()
		{
			if (animationSolution == AnimationSolution.ScriptBased)
			{
				Animator component = GetComponent<Animator>();
				if (component != null)
				{
					UnityEngine.Object.Destroy(component);
				}
			}
			if (base.gameObject.GetComponent<Image>() == null)
			{
				Image image = base.gameObject.AddComponent<Image>();
				image.color = new Color(0f, 0f, 0f, 0f);
				image.raycastTarget = true;
			}
			if (useUINavigation)
			{
				AddUINavigation();
			}
			if (normalCG == null)
			{
				normalCG = new GameObject().AddComponent<CanvasGroup>();
				normalCG.gameObject.AddComponent<RectTransform>();
				normalCG.transform.SetParent(base.transform);
				normalCG.gameObject.name = "Normal";
			}
			if (highlightCG == null)
			{
				highlightCG = new GameObject().AddComponent<CanvasGroup>();
				highlightCG.gameObject.AddComponent<RectTransform>();
				highlightCG.transform.SetParent(base.transform);
				highlightCG.gameObject.name = "Highlight";
			}
			if (disabledCG == null)
			{
				disabledCG = new GameObject().AddComponent<CanvasGroup>();
				disabledCG.gameObject.AddComponent<RectTransform>();
				disabledCG.transform.SetParent(base.transform);
				disabledCG.gameObject.name = "Disabled";
			}
			if (useRipple && rippleParent != null)
			{
				rippleParent.SetActive(value: false);
			}
			else if (!useRipple && rippleParent != null)
			{
				UnityEngine.Object.Destroy(rippleParent);
			}
			StartCoroutine("LayoutFix");
		}

		public void UpdateUI()
		{
			if (!autoFitContent)
			{
				if (mainFitter != null)
				{
					mainFitter.enabled = false;
				}
				if (mainLayout != null)
				{
					mainLayout.enabled = false;
				}
				if (targetFitter != null)
				{
					targetFitter.enabled = false;
					if (targetRect != null)
					{
						targetRect.anchorMin = new Vector2(0f, 0f);
						targetRect.anchorMax = new Vector2(1f, 1f);
						targetRect.offsetMin = new Vector2(0f, 0f);
						targetRect.offsetMax = new Vector2(0f, 0f);
					}
				}
			}
			else
			{
				if (mainFitter != null)
				{
					mainFitter.enabled = true;
				}
				if (mainLayout != null)
				{
					mainLayout.enabled = true;
				}
				if (targetFitter != null)
				{
					targetFitter.enabled = true;
				}
			}
			if (disabledLayout != null)
			{
				disabledLayout.padding = new RectOffset(padding.left, padding.right, padding.top, padding.bottom);
				disabledLayout.spacing = spacing;
			}
			if (normalLayout != null)
			{
				normalLayout.padding = new RectOffset(padding.left, padding.right, padding.top, padding.bottom);
				normalLayout.spacing = spacing;
			}
			if (highlightedLayout != null)
			{
				highlightedLayout.padding = new RectOffset(padding.left, padding.right, padding.top, padding.bottom);
				highlightedLayout.spacing = spacing;
			}
			if (normalCG != null && isInteractable)
			{
				normalCG.alpha = 1f;
			}
			if (disabledCG != null && !isInteractable)
			{
				disabledCG.alpha = 1f;
			}
			if (highlightCG != null)
			{
				highlightCG.alpha = 0f;
			}
			if (enableText)
			{
				if (normalText != null)
				{
					normalText.gameObject.SetActive(value: true);
					normalText.text = buttonText;
					if (!useCustomTextSize)
					{
						normalText.fontSize = textSize;
					}
				}
				if (highlightedText != null)
				{
					highlightedText.gameObject.SetActive(value: true);
					highlightedText.text = buttonText;
					if (!useCustomTextSize)
					{
						highlightedText.fontSize = textSize;
					}
				}
				if (disabledText != null)
				{
					disabledText.gameObject.SetActive(value: true);
					disabledText.text = buttonText;
					if (!useCustomTextSize)
					{
						disabledText.fontSize = textSize;
					}
				}
			}
			else if (!enableText)
			{
				if (normalText != null)
				{
					normalText.gameObject.SetActive(value: false);
				}
				if (highlightedText != null)
				{
					highlightedText.gameObject.SetActive(value: false);
				}
				if (disabledText != null)
				{
					disabledText.gameObject.SetActive(value: false);
				}
			}
			if (enableIcon)
			{
				Vector3 localScale = new Vector3(iconScale, iconScale, iconScale);
				if (normalImage != null)
				{
					normalImage.transform.parent.gameObject.SetActive(value: true);
					normalImage.sprite = buttonIcon;
					normalImage.transform.localScale = localScale;
				}
				if (highlightImage != null)
				{
					highlightImage.transform.parent.gameObject.SetActive(value: true);
					highlightImage.sprite = buttonIcon;
					highlightImage.transform.localScale = localScale;
				}
				if (disabledImage != null)
				{
					disabledImage.transform.parent.gameObject.SetActive(value: true);
					disabledImage.sprite = buttonIcon;
					disabledImage.transform.localScale = localScale;
				}
			}
			else
			{
				if (normalImage != null)
				{
					normalImage.transform.parent.gameObject.SetActive(value: false);
				}
				if (highlightImage != null)
				{
					highlightImage.transform.parent.gameObject.SetActive(value: false);
				}
				if (disabledImage != null)
				{
					disabledImage.transform.parent.gameObject.SetActive(value: false);
				}
			}
			if (Application.isPlaying && base.gameObject.activeInHierarchy)
			{
				if (!isInteractable)
				{
					StartCoroutine("SetDisabled");
				}
				else if (isInteractable && disabledCG.alpha == 1f)
				{
					StartCoroutine("SetNormal");
				}
				StartCoroutine("LayoutFix");
			}
		}

		public void SetText(string text)
		{
			buttonText = text;
			UpdateUI();
		}

		public void SetIcon(Sprite icon)
		{
			buttonIcon = icon;
			UpdateUI();
		}

		public void Interactable(bool value)
		{
			isInteractable = value;
			if (base.gameObject.activeInHierarchy)
			{
				if (!isInteractable)
				{
					StartCoroutine("SetDisabled");
				}
				else if (isInteractable && disabledCG.alpha == 1f)
				{
					StartCoroutine("SetNormal");
				}
			}
		}

		public void AddUINavigation()
		{
			targetButton = base.gameObject.AddComponent<Button>();
			targetButton.transition = Selectable.Transition.None;
			Navigation navigation = new Navigation
			{
				mode = navigationMode
			};
			if (navigationMode == Navigation.Mode.Vertical || navigationMode == Navigation.Mode.Horizontal)
			{
				navigation.wrapAround = wrapAround;
			}
			else if (navigationMode == Navigation.Mode.Explicit)
			{
				StartCoroutine("InitUINavigation", navigation);
				return;
			}
			targetButton.navigation = navigation;
		}

		public void CreateRipple(Vector2 pos)
		{
			if (rippleParent != null)
			{
				GameObject gameObject = new GameObject();
				gameObject.AddComponent<Image>();
				gameObject.GetComponent<Image>().sprite = rippleShape;
				gameObject.name = "Ripple";
				rippleParent.SetActive(value: true);
				gameObject.transform.SetParent(rippleParent.transform);
				if (renderOnTop)
				{
					rippleParent.transform.SetAsLastSibling();
				}
				else
				{
					rippleParent.transform.SetAsFirstSibling();
				}
				if (centered)
				{
					gameObject.transform.localPosition = new Vector2(0f, 0f);
				}
				else
				{
					gameObject.transform.position = pos;
				}
				gameObject.AddComponent<Ripple>();
				Ripple component = gameObject.GetComponent<Ripple>();
				component.speed = speed;
				component.maxSize = maxSize;
				component.startColor = startColor;
				component.transitionColor = transitionColor;
				if (rippleUpdateMode == RippleUpdateMode.Normal)
				{
					component.unscaledTime = false;
				}
				else
				{
					component.unscaledTime = true;
				}
			}
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			if (!isInteractable || eventData.button != PointerEventData.InputButton.Left)
			{
				return;
			}
			if (enableButtonSounds && useClickSound && soundSource != null)
			{
				soundSource.PlayOneShot(clickSound);
			}
			onClick.Invoke();
			if (checkForDoubleClick)
			{
				if (waitingForDoubleClickInput)
				{
					onDoubleClick.Invoke();
					waitingForDoubleClickInput = false;
				}
				else
				{
					waitingForDoubleClickInput = true;
					StopCoroutine("CheckForDoubleClick");
				}
			}
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			if (isInteractable && useRipple && isPointerOn)
			{
				CreateRipple(Mouse.current.position.ReadValue());
			}
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (isInteractable)
			{
				if (enableButtonSounds && useHoverSound && soundSource != null)
				{
					soundSource.PlayOneShot(hoverSound);
				}
				if (animationSolution == AnimationSolution.ScriptBased)
				{
					StartCoroutine("SetHighlight");
				}
				isPointerOn = true;
				onHover.Invoke();
			}
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			if (isInteractable)
			{
				if (animationSolution == AnimationSolution.ScriptBased)
				{
					StartCoroutine("SetNormal");
				}
				isPointerOn = false;
				onLeave.Invoke();
			}
		}

		public void OnSelect(BaseEventData eventData)
		{
			if (isInteractable && animationSolution == AnimationSolution.ScriptBased)
			{
				StartCoroutine("SetHighlight");
			}
		}

		public void OnDeselect(BaseEventData eventData)
		{
			if (isInteractable && animationSolution == AnimationSolution.ScriptBased)
			{
				StartCoroutine("SetNormal");
			}
		}

		public void OnSubmit(BaseEventData eventData)
		{
			if (isInteractable)
			{
				if (animationSolution == AnimationSolution.ScriptBased)
				{
					StartCoroutine("SetNormal");
				}
				onClick.Invoke();
			}
		}

		private IEnumerator LayoutFix()
		{
			yield return new WaitForSecondsRealtime(0.025f);
			LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
			if (disabledCG != null)
			{
				LayoutRebuilder.ForceRebuildLayoutImmediate(disabledCG.GetComponent<RectTransform>());
			}
			if (normalCG != null)
			{
				LayoutRebuilder.ForceRebuildLayoutImmediate(normalCG.GetComponent<RectTransform>());
			}
			if (highlightCG != null)
			{
				LayoutRebuilder.ForceRebuildLayoutImmediate(highlightCG.GetComponent<RectTransform>());
			}
		}

		private IEnumerator SetNormal()
		{
			StopCoroutine("SetHighlight");
			StopCoroutine("SetDisabled");
			while (normalCG.alpha < 0.99f)
			{
				normalCG.alpha += Time.unscaledDeltaTime * fadingMultiplier;
				highlightCG.alpha -= Time.unscaledDeltaTime * fadingMultiplier;
				disabledCG.alpha -= Time.unscaledDeltaTime * fadingMultiplier;
				yield return null;
			}
			normalCG.alpha = 1f;
			highlightCG.alpha = 0f;
			disabledCG.alpha = 0f;
		}

		private IEnumerator SetHighlight()
		{
			StopCoroutine("SetNormal");
			StopCoroutine("SetDisabled");
			while (highlightCG.alpha < 0.99f)
			{
				normalCG.alpha -= Time.unscaledDeltaTime * fadingMultiplier;
				highlightCG.alpha += Time.unscaledDeltaTime * fadingMultiplier;
				disabledCG.alpha -= Time.unscaledDeltaTime * fadingMultiplier;
				yield return null;
			}
			normalCG.alpha = 0f;
			highlightCG.alpha = 1f;
			disabledCG.alpha = 0f;
		}

		private IEnumerator SetDisabled()
		{
			StopCoroutine("SetNormal");
			StopCoroutine("SetHighlight");
			while (disabledCG.alpha < 0.99f)
			{
				normalCG.alpha -= Time.unscaledDeltaTime * fadingMultiplier;
				highlightCG.alpha -= Time.unscaledDeltaTime * fadingMultiplier;
				disabledCG.alpha += Time.unscaledDeltaTime * fadingMultiplier;
				yield return null;
			}
			normalCG.alpha = 0f;
			highlightCG.alpha = 0f;
			disabledCG.alpha = 1f;
		}

		private IEnumerator CheckForDoubleClick()
		{
			yield return new WaitForSecondsRealtime(doubleClickPeriod);
			waitingForDoubleClickInput = false;
		}

		private IEnumerator InitUINavigation(Navigation nav)
		{
			yield return new WaitForSecondsRealtime(1f);
			if (selectOnUp != null)
			{
				nav.selectOnUp = selectOnUp.GetComponent<Selectable>();
			}
			if (selectOnDown != null)
			{
				nav.selectOnDown = selectOnDown.GetComponent<Selectable>();
			}
			if (selectOnLeft != null)
			{
				nav.selectOnLeft = selectOnLeft.GetComponent<Selectable>();
			}
			if (selectOnRight != null)
			{
				nav.selectOnRight = selectOnRight.GetComponent<Selectable>();
			}
			targetButton.navigation = nav;
		}
	}
}
