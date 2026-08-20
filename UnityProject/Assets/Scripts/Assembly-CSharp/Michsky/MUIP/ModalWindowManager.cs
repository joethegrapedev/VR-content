using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Michsky.MUIP
{
	[RequireComponent(typeof(CanvasGroup))]
	public class ModalWindowManager : MonoBehaviour
	{
		public enum StartBehaviour
		{
			None = 0,
			Disable = 1,
			Enable = 2
		}

		public enum CloseBehaviour
		{
			None = 0,
			Disable = 1,
			Destroy = 2
		}

		public Image windowIcon;

		public TextMeshProUGUI windowTitle;

		public TextMeshProUGUI windowDescription;

		public ButtonManager confirmButton;

		public ButtonManager cancelButton;

		public Animator mwAnimator;

		public Sprite icon;

		public string titleText = "Title";

		[TextArea]
		public string descriptionText = "Description here";

		public UnityEvent onOpen;

		public UnityEvent onConfirm;

		public UnityEvent onCancel;

		public bool useCustomContent;

		public bool isOn;

		public bool closeOnCancel = true;

		public bool closeOnConfirm = true;

		public bool showCancelButton = true;

		public bool showConfirmButton = true;

		public StartBehaviour startBehaviour = StartBehaviour.Disable;

		public CloseBehaviour closeBehaviour = CloseBehaviour.Disable;

		private void Awake()
		{
			isOn = false;
			if (mwAnimator == null)
			{
				mwAnimator = base.gameObject.GetComponent<Animator>();
			}
			if (closeOnCancel)
			{
				onCancel.AddListener(CloseWindow);
			}
			if (closeOnConfirm)
			{
				onConfirm.AddListener(CloseWindow);
			}
			if (confirmButton != null)
			{
				confirmButton.onClick.AddListener(onConfirm.Invoke);
			}
			if (cancelButton != null)
			{
				cancelButton.onClick.AddListener(onCancel.Invoke);
			}
			if (startBehaviour == StartBehaviour.Disable)
			{
				isOn = false;
				base.gameObject.SetActive(value: false);
			}
			else if (startBehaviour == StartBehaviour.Enable)
			{
				isOn = false;
				OpenWindow();
			}
			UpdateUI();
		}

		public void UpdateUI()
		{
			if (!useCustomContent)
			{
				if (windowIcon != null)
				{
					windowIcon.sprite = icon;
				}
				if (windowTitle != null)
				{
					windowTitle.text = titleText;
				}
				if (windowDescription != null)
				{
					windowDescription.text = descriptionText;
				}
				if (showCancelButton && cancelButton != null)
				{
					cancelButton.gameObject.SetActive(value: true);
				}
				else if (cancelButton != null)
				{
					cancelButton.gameObject.SetActive(value: false);
				}
				if (showConfirmButton && confirmButton != null)
				{
					confirmButton.gameObject.SetActive(value: true);
				}
				else if (confirmButton != null)
				{
					confirmButton.gameObject.SetActive(value: false);
				}
			}
		}

		public void Open()
		{
			if (!isOn)
			{
				StopCoroutine("DisableObject");
				base.gameObject.SetActive(value: true);
				isOn = true;
				onOpen.Invoke();
				mwAnimator.Play("Fade-in");
			}
		}

		public void Close()
		{
			if (isOn)
			{
				isOn = false;
				mwAnimator.Play("Fade-out");
				StartCoroutine("DisableObject");
			}
		}

		public void OpenWindow()
		{
			Open();
		}

		public void CloseWindow()
		{
			Close();
		}

		public void AnimateWindow()
		{
			if (!isOn)
			{
				StopCoroutine("DisableObject");
				isOn = true;
				base.gameObject.SetActive(value: true);
				mwAnimator.Play("Fade-in");
			}
			else
			{
				isOn = false;
				mwAnimator.Play("Fade-out");
				StartCoroutine("DisableObject");
			}
		}

		private IEnumerator DisableObject()
		{
			yield return new WaitForSeconds(1f);
			if (closeBehaviour == CloseBehaviour.Disable)
			{
				base.gameObject.SetActive(value: false);
			}
			else if (closeBehaviour == CloseBehaviour.Destroy)
			{
				Object.Destroy(base.gameObject);
			}
		}
	}
}
