using UnityEngine;

public class TaskStatus : MonoBehaviour
{
	public MyRuntimeTest excelScript;

	public bool task1;

	public bool task2;

	public bool task3;

	public bool task4;

	public bool task5;

	public bool task6;

	public bool task7;

	public bool task8;

	public bool task9;

	public bool task10;

	public bool task11;

	public bool task12;

	public bool task13;

	private void Start()
	{
	}

	public void checkTask()
	{
		if ((task1 && task2 && task3 && task4 && task5 && task6 && task7 && task8 && task9 && task10 && task11 && task12 && task13) || excelScript.countdown >= TimerSettings.setTime)
		{
			Debug.Log("finished");
		}
		else
		{
			Debug.Log("not finished yet");
		}
	}
}
