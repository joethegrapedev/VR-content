using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Autohand
{
	public class AutoInputModule : BaseInputModule
	{
		private List<HandCanvasPointer> pointers = new List<HandCanvasPointer>();

		private PointerEventData[] eventDatas;

		private AutoInputModule _instance;

		private bool _isDestroyed;

		public AutoInputModule Instance
		{
			get
			{
				if (_isDestroyed)
				{
					return null;
				}
				if (_instance == null)
				{
					if (!(_instance = Object.FindObjectOfType<AutoInputModule>()))
					{
						_instance = new GameObject().AddComponent<AutoInputModule>();
						_instance.transform.parent = AutoHandExtensions.transformParent;
					}
					EventSystem[] array = null;
					BaseInputModule[] array2 = Object.FindObjectsOfType<BaseInputModule>();
					if (array2.Length > 1)
					{
						for (int num = array2.Length - 1; num >= 0; num--)
						{
							if (!array2[num].gameObject.GetComponent<AutoInputModule>())
							{
								Object.Destroy(array2[num]);
							}
							Debug.LogWarning("AUTO HAND:  REMOVING ADDITIONAL EVENT SYSTEMS FROM THE SCENE");
						}
					}
					array = Object.FindObjectsOfType<EventSystem>();
					if (array.Length > 1)
					{
						for (int num2 = array.Length - 1; num2 >= 0; num2--)
						{
							if (!array[num2].gameObject.GetComponent<AutoInputModule>())
							{
								Object.Destroy(array[num2]);
							}
							Debug.LogWarning("AUTO HAND:  REMOVING ADDITIONAL EVENT SYSTEMS FROM THE SCENE");
						}
					}
				}
				return _instance;
			}
		}

		protected override void Awake()
		{
			base.Awake();
		}

		protected override void OnDestroy()
		{
			_isDestroyed = true;
		}

		public int AddPointer(HandCanvasPointer pointer)
		{
			if (!pointers.Contains(pointer))
			{
				pointers.Add(pointer);
				eventDatas = new PointerEventData[pointers.Count];
				for (int i = 0; i < eventDatas.Length; i++)
				{
					eventDatas[i] = new PointerEventData(base.eventSystem);
					eventDatas[i].delta = Vector2.zero;
					eventDatas[i].position = new Vector2(Screen.width / 2, Screen.height / 2);
				}
			}
			return pointers.IndexOf(pointer);
		}

		public void RemovePointer(HandCanvasPointer pointer)
		{
			if (pointers.Contains(pointer))
			{
				pointers.Remove(pointer);
			}
			foreach (HandCanvasPointer pointer2 in pointers)
			{
				pointer2.SetIndex(pointers.IndexOf(pointer2));
			}
			eventDatas = new PointerEventData[pointers.Count];
			for (int i = 0; i < eventDatas.Length; i++)
			{
				eventDatas[i] = new PointerEventData(base.eventSystem);
				eventDatas[i].delta = Vector2.zero;
				eventDatas[i].position = new Vector2(Screen.width / 2, Screen.height / 2);
			}
		}

		public override void Process()
		{
			for (int i = 0; i < pointers.Count; i++)
			{
				try
				{
					if (pointers[i] != null && pointers[i].enabled)
					{
						pointers[i].Preprocess();
						base.eventSystem.RaycastAll(eventDatas[i], m_RaycastResultCache);
						eventDatas[i].pointerCurrentRaycast = BaseInputModule.FindFirstRaycast(m_RaycastResultCache);
						HandlePointerExitAndEnter(eventDatas[i], eventDatas[i].pointerCurrentRaycast.gameObject);
						ExecuteEvents.Execute(eventDatas[i].pointerDrag, eventDatas[i], ExecuteEvents.dragHandler);
					}
				}
				catch
				{
				}
			}
		}

		public void ProcessPress(int index)
		{
			pointers[index].Preprocess();
			eventDatas[index].pointerPressRaycast = eventDatas[index].pointerCurrentRaycast;
			eventDatas[index].pointerPress = ExecuteEvents.GetEventHandler<IPointerClickHandler>(eventDatas[index].pointerPressRaycast.gameObject);
			eventDatas[index].pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(eventDatas[index].pointerPressRaycast.gameObject);
			ExecuteEvents.Execute(eventDatas[index].pointerPress, eventDatas[index], ExecuteEvents.pointerDownHandler);
			ExecuteEvents.Execute(eventDatas[index].pointerDrag, eventDatas[index], ExecuteEvents.beginDragHandler);
		}

		public void ProcessRelease(int index)
		{
			pointers[index].Preprocess();
			GameObject eventHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(eventDatas[index].pointerCurrentRaycast.gameObject);
			if (eventDatas[index].pointerPress == eventHandler)
			{
				ExecuteEvents.Execute(eventDatas[index].pointerPress, eventDatas[index], ExecuteEvents.pointerClickHandler);
			}
			ExecuteEvents.Execute(eventDatas[index].pointerPress, eventDatas[index], ExecuteEvents.pointerUpHandler);
			ExecuteEvents.Execute(eventDatas[index].pointerDrag, eventDatas[index], ExecuteEvents.endDragHandler);
			eventDatas[index].pointerPress = null;
			eventDatas[index].pointerDrag = null;
			eventDatas[index].pointerCurrentRaycast.Clear();
		}

		public PointerEventData GetData(int index)
		{
			return eventDatas[index];
		}
	}
}
