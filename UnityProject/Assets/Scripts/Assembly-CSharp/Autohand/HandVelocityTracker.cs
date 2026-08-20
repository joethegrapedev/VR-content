using System.Collections.Generic;
using UnityEngine;

namespace Autohand
{
	public class HandVelocityTracker
	{
		private HandBase hand;

		private float minThrowVelocity;

		protected List<VelocityTimePair> m_ThrowVelocityList = new List<VelocityTimePair>();

		protected List<VelocityTimePair> m_ThrowAngleVelocityList = new List<VelocityTimePair>();

		private float disableTime;

		private float disableSeconds;

		public void ClearThrow()
		{
			m_ThrowVelocityList.Clear();
			m_ThrowAngleVelocityList.Clear();
		}

		public void Disable(float seconds)
		{
			disableTime = Time.realtimeSinceStartup;
			disableSeconds = seconds;
			ClearThrow();
		}

		public HandVelocityTracker(HandBase hand)
		{
			this.hand = hand;
		}

		public void UpdateThrowing()
		{
			if (disableTime + disableSeconds > Time.realtimeSinceStartup)
			{
				if (m_ThrowVelocityList.Count > 0)
				{
					m_ThrowVelocityList.Clear();
					m_ThrowAngleVelocityList.Clear();
				}
				return;
			}
			if (hand.holdingObj == null || hand.IsGrabbing())
			{
				if (m_ThrowVelocityList.Count > 0)
				{
					m_ThrowVelocityList.Clear();
					m_ThrowAngleVelocityList.Clear();
				}
				return;
			}
			m_ThrowVelocityList.Add(new VelocityTimePair
			{
				time = Time.realtimeSinceStartup,
				velocity = ((hand.holdingObj.body == null) ? Vector3.zero : hand.holdingObj.body.velocity)
			});
			for (int num = m_ThrowVelocityList.Count - 1; num >= 0; num--)
			{
				if (Time.realtimeSinceStartup - m_ThrowVelocityList[num].time >= hand.throwVelocityExpireTime)
				{
					m_ThrowVelocityList.RemoveAt(num);
				}
			}
			m_ThrowAngleVelocityList.Add(new VelocityTimePair
			{
				time = Time.realtimeSinceStartup,
				velocity = ((hand.holdingObj.body == null) ? Vector3.zero : hand.holdingObj.body.angularVelocity)
			});
			for (int num2 = m_ThrowAngleVelocityList.Count - 1; num2 >= 0; num2--)
			{
				if (Time.realtimeSinceStartup - m_ThrowAngleVelocityList[num2].time >= hand.throwAngularVelocityExpireTime)
				{
					m_ThrowAngleVelocityList.RemoveAt(num2);
				}
			}
		}

		public Vector3 ThrowVelocity()
		{
			if (hand.IsGrabbing() || hand.holdingObj == null)
			{
				return Vector3.zero;
			}
			Vector3 zero = Vector3.zero;
			if (m_ThrowVelocityList.Count > 0)
			{
				foreach (VelocityTimePair throwVelocity in m_ThrowVelocityList)
				{
					zero += throwVelocity.velocity;
				}
				zero /= (float)m_ThrowVelocityList.Count;
			}
			Vector3 result = zero * hand.holdingObj.throwPower;
			if (!(result.magnitude > minThrowVelocity))
			{
				return Vector3.zero;
			}
			return result;
		}

		public Vector3 ThrowAngularVelocity()
		{
			if (hand.IsGrabbing() || hand.holdingObj == null)
			{
				return Vector3.zero;
			}
			Vector3 zero = Vector3.zero;
			if (m_ThrowAngleVelocityList.Count > 0)
			{
				foreach (VelocityTimePair throwAngleVelocity in m_ThrowAngleVelocityList)
				{
					zero += throwAngleVelocity.velocity;
				}
				zero /= (float)m_ThrowAngleVelocityList.Count;
			}
			zero *= Mathf.Sqrt(hand.throwPower) / 2f;
			if (!(zero.magnitude > minThrowVelocity))
			{
				return Vector3.zero;
			}
			return zero;
		}
	}
}
