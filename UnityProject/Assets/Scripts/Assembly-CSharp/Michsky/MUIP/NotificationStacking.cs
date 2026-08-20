using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Michsky.MUIP
{
	[AddComponentMenu("Modern UI Pack/Notification/Notification Stacking")]
	public class NotificationStacking : MonoBehaviour
	{
		public List<NotificationManager> notifications = new List<NotificationManager>();

		[HideInInspector]
		public bool enableUpdating;

		[Header("Settings")]
		public float delay = 1f;

		private int currentNotification;

		private void Update()
		{
			if (!enableUpdating)
			{
				return;
			}
			try
			{
				notifications[currentNotification].gameObject.SetActive(value: true);
				if (notifications[currentNotification].notificationAnimator.GetCurrentAnimatorStateInfo(0).IsName("Wait"))
				{
					notifications[currentNotification].OpenNotification();
					StartCoroutine("StartNotification");
					enableUpdating = false;
				}
				if (currentNotification >= notifications.Count)
				{
					enableUpdating = false;
					currentNotification = 0;
				}
			}
			catch
			{
				enableUpdating = false;
				currentNotification = 0;
				notifications.Clear();
			}
		}

		private IEnumerator StartNotification()
		{
			yield return new WaitForSeconds(notifications[currentNotification].timer + delay);
			Object.Destroy(notifications[currentNotification].gameObject);
			enableUpdating = true;
			currentNotification++;
			StopCoroutine("StartNotification");
		}
	}
}
