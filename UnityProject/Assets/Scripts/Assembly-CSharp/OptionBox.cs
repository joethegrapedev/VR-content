using UnityEngine;
using UnityEngine.UI;

public class OptionBox : MonoBehaviour
{
	public int maxPoint;

	public PlayerScore scorescript;

	public MyRuntimeTest savescript;

	public TaskStatus taskStatus;

	public GameObject taskObject;

	public GameObject interactionMenu;

	public GameObject greenHighlight;

	public GameObject redHighlight;

	public GameObject yellowHighlight;

	public Image checkMarker;

	private bool wrongSelected;

	private RectTransform checkMarkerRectTransform;

	private RectTransform crossImageRectTransform;

	private string taskName;

	private void Start()
	{
		wrongSelected = false;
		maxPoint = 40;
		checkMarkerRectTransform = checkMarker.GetComponent<RectTransform>();
		taskName = taskObject.name;
	}

	public void CorrectOption()
	{
		GameAudioCues.TaskCorrect(transform.position);
		scorescript.AddTotal(maxPoint);
		LeanTween.alpha(checkMarkerRectTransform, 1f, 1f);
		interactionMenu.SetActive(value: false);
		savescript.tasks++;
		if (!wrongSelected)
		{
			greenHighlight.SetActive(value: true);
			redHighlight.SetActive(value: false);
			yellowHighlight.SetActive(value: false);
		}
		else
		{
			greenHighlight.SetActive(value: false);
			redHighlight.SetActive(value: false);
			yellowHighlight.SetActive(value: true);
		}
		updateTaskStatus();
	}

	public void WrongOption()
	{
		GameAudioCues.TaskWrong(transform.position);
		maxPoint -= 10;
		greenHighlight.SetActive(value: false);
		redHighlight.SetActive(value: false);
		yellowHighlight.SetActive(value: true);
		wrongSelected = true;
	}

	private void updateTaskStatus()
	{
		switch (taskName)
		{
		case "Task1":
			taskStatus.task1 = true;
			taskStatus.checkTask();
			break;
		case "Task2":
			taskStatus.task2 = true;
			taskStatus.checkTask();
			break;
		case "Task3":
			taskStatus.task3 = true;
			taskStatus.checkTask();
			break;
		case "Task4":
			taskStatus.task4 = true;
			taskStatus.checkTask();
			break;
		case "Task5":
			taskStatus.task5 = true;
			taskStatus.checkTask();
			break;
		case "Task6":
			taskStatus.task6 = true;
			taskStatus.checkTask();
			break;
		case "Task7":
			taskStatus.task7 = true;
			taskStatus.checkTask();
			break;
		case "Task8":
			taskStatus.task8 = true;
			taskStatus.checkTask();
			break;
		case "Task9":
			taskStatus.task9 = true;
			taskStatus.checkTask();
			break;
		case "Task10":
			taskStatus.task10 = true;
			taskStatus.checkTask();
			break;
		case "Task11":
			taskStatus.task11 = true;
			taskStatus.checkTask();
			break;
		case "Task12":
			taskStatus.task12 = true;
			taskStatus.checkTask();
			break;
		case "Task13":
			taskStatus.task13 = true;
			taskStatus.checkTask();
			break;
		}
	}
}
