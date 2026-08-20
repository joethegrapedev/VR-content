using System;
using TMPro;
using UnityEngine;

namespace Autohand.Demo
{
	public class LeverTextChanger : MonoBehaviour
	{
		public TextMeshPro text;

		public PhysicsGadgetHingeAngleReader sliderReader;

		private void Update()
		{
			text.text = Math.Round(sliderReader.GetValue(), 2).ToString();
		}
	}
}
