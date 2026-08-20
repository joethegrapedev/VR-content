using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Michsky.MUIP
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Animator))]
	public class NotificationManager : MonoBehaviour
	{
		public enum StartBehaviour
		{
			None = 0,
			Disable = 1
		}

		public enum CloseBehaviour
		{
			None = 0,
			Disable = 1,
			Destroy = 2
		}

		public Sprite icon;

		public string title = "Notification Title";

		[TextArea]
		public string description = "Notification description";

		public Animator notificationAnimator;

		public Image iconObj;

		public TextMeshProUGUI titleObj;

		public TextMeshProUGUI descriptionObj;

		public bool enableTimer = true;

		public float timer = 3f;

		public bool useCustomContent;

		public bool useStacking;

		[HideInInspector]
		public bool isOn;

		public StartBehaviour startBehaviour = StartBehaviour.Disable;

		public CloseBehaviour closeBehaviour = CloseBehaviour.Disable;

		public UnityEvent onOpen;

		public UnityEvent onClose;

		private void Awake()
		{
			isOn = false;
			if (!useCustomContent)
			{
				UpdateUI();
			}
			if (notificationAnimator == null)
			{
				notificationAnimator = base.gameObject.GetComponent<Animator>();
			}
			if (startBehaviour == StartBehaviour.Disable)
			{
				base.gameObject.SetActive(value: false);
			}
			if (useStacking)
			{
				try
				{
					NotificationStacking componentInParent = base.transform.GetComponentInParent<NotificationStacking>();
					componentInParent.notifications.Add(this);
					componentInParent.enableUpdating = true;
				}
				catch
				{
					Debug.LogError("<b>[Notification]</b> 'Stacking' is enabled but 'Notification Stacking' cannot be found in parent.", this);
				}
			}
		}

		public void Open()
		{
			if (!isOn)
			{
				base.gameObject.SetActive(value: true);
				isOn = true;
				StopCoroutine("StartTimer");
				StopCoroutine("DisableNotification");
				notificationAnimator.Play("In");
				onOpen.Invoke();
				if (enableTimer)
				{
					StartCoroutine("StartTimer");
				}
			}
		}

		public void Close()
		{
			if (isOn)
			{
				isOn = false;
				notificationAnimator.Play("Out");
				onClose.Invoke();
				StartCoroutine("DisableNotification");
			}
		}

		public void OpenNotification()
		{
			Open();
		}

		public void CloseNotification()
		{
			Close();
		}

		public void UpdateUI()
		{
			if (iconObj != null)
			{
				iconObj.sprite = icon;
			}
			if (titleObj != null)
			{
				titleObj.text = title;
			}
			if (descriptionObj != null)
			{
				descriptionObj.text = description;
			}
		}

		private IEnumerator StartTimer()
		{
			yield return new WaitForSeconds(timer);
			CloseNotification();
			StartCoroutine("DisableNotification");
		}

		private IEnumerator DisableNotification()
		{
			yield return new WaitForSeconds(1f);
			if (closeBehaviour == CloseBehaviour.Disable)
			{
				base.gameObject.SetActive(value: false);
				isOn = false;
			}
			else if (closeBehaviour == CloseBehaviour.Destroy)
			{
				Object.Destroy(base.gameObject);
			}
		}
	}
}
