using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Michsky.MUIP
{
	[RequireComponent(typeof(TMP_InputField))]
	[RequireComponent(typeof(Animator))]
	public class CustomInputField : MonoBehaviour
	{
		[Header("Resources")]
		public TMP_InputField inputText;

		public Animator inputFieldAnimator;

		[Header("Settings")]
		public bool processSubmit;

		public bool clearOnSubmit = true;

		[Header("Events")]
		public UnityEvent onSubmit;

		private string inAnim = "In";

		private string outAnim = "Out";

		private string instaInAnim = "Instant In";

		private string instaOutAnim = "Instant Out";

		private void Awake()
		{
			if (inputText == null)
			{
				inputText = base.gameObject.GetComponent<TMP_InputField>();
			}
			if (inputFieldAnimator == null)
			{
				inputFieldAnimator = base.gameObject.GetComponent<Animator>();
			}
			inputText.onSelect.AddListener(delegate
			{
				AnimateIn();
			});
			inputText.onEndEdit.AddListener(delegate
			{
				AnimateOut();
			});
			UpdateStateInstant();
		}

		private void OnEnable()
		{
			if (!(inputText == null))
			{
				inputText.ForceLabelUpdate();
				UpdateStateInstant();
				if (base.gameObject.activeInHierarchy)
				{
					StartCoroutine("DisableAnimator");
				}
			}
		}

		private void Update()
		{
			if (processSubmit && !string.IsNullOrEmpty(inputText.text) && !(EventSystem.current.currentSelectedGameObject != inputText.gameObject) && Keyboard.current.enterKey.wasPressedThisFrame)
			{
				onSubmit.Invoke();
				if (clearOnSubmit)
				{
					inputText.text = "";
				}
			}
		}

		public void AnimateIn()
		{
			StopCoroutine("DisableAnimator");
			if (inputFieldAnimator.gameObject.activeInHierarchy && inputText.text.Length == 0)
			{
				inputFieldAnimator.enabled = true;
				inputFieldAnimator.Play(inAnim);
				StartCoroutine("DisableAnimator");
			}
		}

		public void AnimateOut()
		{
			if (inputFieldAnimator.gameObject.activeInHierarchy)
			{
				inputFieldAnimator.enabled = true;
				if (inputText.text.Length == 0)
				{
					inputFieldAnimator.Play(outAnim);
				}
				StartCoroutine("DisableAnimator");
			}
		}

		public void UpdateState()
		{
			if (inputText.text.Length == 0)
			{
				AnimateOut();
			}
			else
			{
				AnimateIn();
			}
		}

		public void UpdateStateInstant()
		{
			if (inputText.text.Length == 0)
			{
				inputFieldAnimator.Play(instaOutAnim);
			}
			else
			{
				inputFieldAnimator.Play(instaInAnim);
			}
		}

		private IEnumerator DisableAnimator()
		{
			yield return new WaitForSeconds(1f);
			inputFieldAnimator.enabled = false;
		}
	}
}
