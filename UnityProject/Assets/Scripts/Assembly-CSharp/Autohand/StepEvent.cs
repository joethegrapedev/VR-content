using System;
using UnityEngine.Events;

namespace Autohand
{
	[Serializable]
	public struct StepEvent
	{
		public int step;

		public UnityEvent OnStepEnter;

		public UnityEvent OnStepExit;
	}
}
