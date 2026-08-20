using System;
using Autohand;
using UnityEngine;

[RequireComponent(typeof(PlacePoint))]
public class PlacePointEventDebugger : MonoBehaviour
{
	private PlacePoint placePoint;

	private void OnEnable()
	{
		placePoint = GetComponent<PlacePoint>();
		PlacePoint obj = placePoint;
		obj.OnPlaceEvent = (PlacePointEvent)Delegate.Combine(obj.OnPlaceEvent, (PlacePointEvent)delegate
		{
			Debug.Log("On Place: " + Time.time);
		});
		PlacePoint obj2 = placePoint;
		obj2.OnRemoveEvent = (PlacePointEvent)Delegate.Combine(obj2.OnRemoveEvent, (PlacePointEvent)delegate
		{
			Debug.Log("On Remove: " + Time.time);
		});
		PlacePoint obj3 = placePoint;
		obj3.OnHighlightEvent = (PlacePointEvent)Delegate.Combine(obj3.OnHighlightEvent, (PlacePointEvent)delegate
		{
			Debug.Log("On Highlight: " + Time.time);
		});
		PlacePoint obj4 = placePoint;
		obj4.OnStopHighlightEvent = (PlacePointEvent)Delegate.Combine(obj4.OnStopHighlightEvent, (PlacePointEvent)delegate
		{
			Debug.Log("On Stop Highlight: " + Time.time);
		});
	}

	private void OnDisable()
	{
		placePoint = GetComponent<PlacePoint>();
		PlacePoint obj = placePoint;
		obj.OnPlaceEvent = (PlacePointEvent)Delegate.Remove(obj.OnPlaceEvent, (PlacePointEvent)delegate
		{
			Debug.Log("On Place: " + Time.time);
		});
		PlacePoint obj2 = placePoint;
		obj2.OnRemoveEvent = (PlacePointEvent)Delegate.Remove(obj2.OnRemoveEvent, (PlacePointEvent)delegate
		{
			Debug.Log("On Remove: " + Time.time);
		});
		PlacePoint obj3 = placePoint;
		obj3.OnHighlightEvent = (PlacePointEvent)Delegate.Remove(obj3.OnHighlightEvent, (PlacePointEvent)delegate
		{
			Debug.Log("On Highlight: " + Time.time);
		});
		PlacePoint obj4 = placePoint;
		obj4.OnStopHighlightEvent = (PlacePointEvent)Delegate.Remove(obj4.OnStopHighlightEvent, (PlacePointEvent)delegate
		{
			Debug.Log("On Stop Highlight: " + Time.time);
		});
	}
}
