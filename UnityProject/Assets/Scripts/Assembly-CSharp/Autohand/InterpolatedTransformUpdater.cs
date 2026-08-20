using UnityEngine;

namespace Autohand
{
	[DefaultExecutionOrder(100)]
	public class InterpolatedTransformUpdater : MonoBehaviour
	{
		private InterpolatedTransform _interpolatedTransform;

		private InterpolatedTransform interpolatedTransform
		{
			get
			{
				if (_interpolatedTransform == null)
				{
					_interpolatedTransform = GetComponent<InterpolatedTransform>();
				}
				return _interpolatedTransform;
			}
		}

		private void FixedUpdate()
		{
			interpolatedTransform?.LateFixedUpdate();
		}
	}
}
