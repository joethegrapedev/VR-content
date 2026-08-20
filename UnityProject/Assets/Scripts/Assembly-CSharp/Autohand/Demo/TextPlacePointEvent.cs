using System;
using UnityEngine;

namespace Autohand.Demo
{
	public class TextPlacePointEvent : MonoBehaviour
	{
		public TextChanger changer;

		public PlacePoint point;

		public float fadeTime = 5f;

		[TextArea]
		public string placeMessage;

		[TextArea]
		public string highlightMessage;

		private void Start()
		{
			if (point == null && GetComponent<PlacePoint>() != null)
			{
				point = GetComponent<PlacePoint>();
			}
			PlacePoint placePoint = point;
			placePoint.OnPlaceEvent = (PlacePointEvent)Delegate.Combine(placePoint.OnPlaceEvent, new PlacePointEvent(OnGrab));
			PlacePoint placePoint2 = point;
			placePoint2.OnHighlightEvent = (PlacePointEvent)Delegate.Combine(placePoint2.OnHighlightEvent, new PlacePointEvent(OnHighlight));
		}

		private void OnGrab(PlacePoint hand, Grabbable grab)
		{
			changer.UpdateText(placeMessage);
		}

		private void OnHighlight(PlacePoint hand, Grabbable grab)
		{
			changer.UpdateText(highlightMessage);
		}
	}
}
