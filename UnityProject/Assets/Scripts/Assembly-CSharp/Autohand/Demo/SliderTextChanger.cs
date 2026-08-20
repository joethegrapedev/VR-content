using System;
using TMPro;
using UnityEngine;

namespace Autohand.Demo
{
	public class SliderTextChanger : MonoBehaviour
	{
		public TextMeshPro text;

		public PhysicsGadgetConfigurableLimitReader sliderReader;

		private void Update()
		{
			text.text = Math.Round(sliderReader.GetValue(), 2).ToString();
		}
	}
}
