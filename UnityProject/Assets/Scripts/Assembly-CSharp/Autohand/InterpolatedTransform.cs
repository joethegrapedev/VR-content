using System.Collections;
using UnityEngine;

namespace Autohand
{
	[DefaultExecutionOrder(-50)]
	[RequireComponent(typeof(InterpolatedTransformUpdater))]
	public class InterpolatedTransform : MonoBehaviour
	{
		private struct TransformData
		{
			public Vector3 position;

			public Quaternion rotation;

			public Vector3 scale;

			public TransformData(Vector3 position, Quaternion rotation, Vector3 scale)
			{
				this.position = position;
				this.rotation = rotation;
				this.scale = scale;
			}
		}

		private TransformData[] m_lastTransforms;

		private int m_newTransformIndex;

		private bool run = true;

		private Coroutine disableForTime;

		private void OnEnable()
		{
			ForgetPreviousTransforms();
		}

		public void ForgetPreviousTransforms()
		{
			m_lastTransforms = new TransformData[2];
			TransformData transformData = new TransformData(base.transform.localPosition, base.transform.localRotation, base.transform.localScale);
			m_lastTransforms[0] = transformData;
			m_lastTransforms[1] = transformData;
			m_newTransformIndex = 0;
		}

		private void FixedUpdate()
		{
			if (run)
			{
				TransformData transformData = m_lastTransforms[m_newTransformIndex];
				base.transform.localPosition = transformData.position;
				base.transform.localRotation = transformData.rotation;
				base.transform.localScale = transformData.scale;
			}
		}

		public void LateFixedUpdate()
		{
			m_newTransformIndex = OldTransformIndex();
			m_lastTransforms[m_newTransformIndex] = new TransformData(base.transform.localPosition, base.transform.localRotation, base.transform.localScale);
		}

		private void Update()
		{
			TransformData transformData = m_lastTransforms[m_newTransformIndex];
			TransformData transformData2 = m_lastTransforms[OldTransformIndex()];
			if (run)
			{
				base.transform.localPosition = Vector3.Lerp(transformData2.position, transformData.position, InterpolationController.InterpolationFactor);
				base.transform.localRotation = Quaternion.Slerp(transformData2.rotation, transformData.rotation, InterpolationController.InterpolationFactor);
				base.transform.localScale = Vector3.Lerp(transformData2.scale, transformData.scale, InterpolationController.InterpolationFactor);
			}
		}

		public void DisableForSeconds(float time)
		{
			if (disableForTime != null)
			{
				StopCoroutine(disableForTime);
				disableForTime = null;
			}
			if (base.gameObject.activeInHierarchy)
			{
				disableForTime = StartCoroutine(Disable(time));
			}
		}

		private IEnumerator Disable(float time)
		{
			run = false;
			yield return new WaitForSeconds(time);
			run = true;
		}

		private int OldTransformIndex()
		{
			if (m_newTransformIndex != 0)
			{
				return 0;
			}
			return 1;
		}
	}
}
