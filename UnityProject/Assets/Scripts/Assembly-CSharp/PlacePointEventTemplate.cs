using System;
using Autohand;
using UnityEngine;

public class PlacePointEventTemplate : MonoBehaviour
{
	public PlacePoint placePoint;

	private void OnEnable()
	{
		PlacePoint obj = placePoint;
		obj.OnPlaceEvent = (PlacePointEvent)Delegate.Combine(obj.OnPlaceEvent, new PlacePointEvent(OnPlace));
		PlacePoint obj2 = placePoint;
		obj2.OnRemoveEvent = (PlacePointEvent)Delegate.Combine(obj2.OnRemoveEvent, new PlacePointEvent(OnPlace));
		PlacePoint obj3 = placePoint;
		obj3.OnHighlightEvent = (PlacePointEvent)Delegate.Combine(obj3.OnHighlightEvent, new PlacePointEvent(OnHighlight));
		PlacePoint obj4 = placePoint;
		obj4.OnStopHighlightEvent = (PlacePointEvent)Delegate.Combine(obj4.OnStopHighlightEvent, new PlacePointEvent(OnStopHighlight));
	}

	private void OnDisable()
	{
		PlacePoint obj = placePoint;
		obj.OnPlaceEvent = (PlacePointEvent)Delegate.Remove(obj.OnPlaceEvent, new PlacePointEvent(OnPlace));
		PlacePoint obj2 = placePoint;
		obj2.OnRemoveEvent = (PlacePointEvent)Delegate.Remove(obj2.OnRemoveEvent, new PlacePointEvent(OnPlace));
		PlacePoint obj3 = placePoint;
		obj3.OnHighlightEvent = (PlacePointEvent)Delegate.Remove(obj3.OnHighlightEvent, new PlacePointEvent(OnHighlight));
		PlacePoint obj4 = placePoint;
		obj4.OnStopHighlightEvent = (PlacePointEvent)Delegate.Remove(obj4.OnStopHighlightEvent, new PlacePointEvent(OnStopHighlight));
	}

	public void OnPlace(PlacePoint point, Grabbable grab)
	{
	}

	public void OnRemove(PlacePoint point, Grabbable grab)
	{
	}

	public void OnHighlight(PlacePoint point, Grabbable grab)
	{
	}

	public void OnStopHighlight(PlacePoint point, Grabbable grab)
	{
	}
}
