using System;
using System.Collections;
using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using TMPro;
using UnityEngine;

public class MyRuntimeTest : MonoBehaviour
{
	public float countdown;

	public int tasks = 6;

	public TextMeshProUGUI TMP_Text;

	public Leaderboard excelScript;

	public PlayerScore scorescript;

	public TaskStatus taskScript;

	public GameObject player;

	public Transform classroomPosition;

	public bool finished = true;

	public AudioClip teleportAnnouncement;

	public AudioSource audioSource;

	private string excelName;

	private string path;

	private bool played;

	protected void Start()
	{
		finished = true;
		path = SavePaths.OutputDirectory + "/";
		if (TimerSettings.setTime == 180f)
		{
			excelName = "RSAF_VIP_Leaderboard.xls";
		}
		else if (TimerSettings.setTime == 600f)
		{
			excelName = "RSAF_Leaderboard.xls";
		}
		RunTimeTest();
	}

	protected void Update()
	{
		if (countdown >= TimerSettings.setTime && tasks != 13 && !played)
		{
			UpdateExcel();
			played = true;
		}
		else if (countdown >= 0f && countdown < TimerSettings.setTime && !played)
		{
			if (tasks == 13)
			{
				UpdateExcel();
				played = true;
			}
			else
			{
				countdown += Time.deltaTime;
				DisplayTime(countdown);
			}
		}
	}

	private void DisplayTime(float timeToDisplay)
	{
		float num = Mathf.FloorToInt(timeToDisplay / 60f);
		float num2 = Mathf.FloorToInt(timeToDisplay % 60f);
		TMP_Text.text = $"{num:00}:{num2:00}";
	}

	private void RunTimeTest()
	{
		DateTime now = DateTime.Now;
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
		if (File.Exists(path + excelName))
		{
			HSSFWorkbook hSSFWorkbook;
			using (FileStream fileStream = new FileStream(path + excelName, FileMode.Open, FileAccess.Read))
			{
				hSSFWorkbook = new HSSFWorkbook(fileStream);
				fileStream.Close();
			}
			hSSFWorkbook.GetSheetAt(0);
			using FileStream fileStream2 = new FileStream(path + excelName, FileMode.Open, FileAccess.Write);
			hSSFWorkbook.Write(fileStream2);
			fileStream2.Close();
			return;
		}
		HSSFWorkbook hSSFWorkbook2 = new HSSFWorkbook();
		ISheet sheet = ((IWorkbook)hSSFWorkbook2).CreateSheet("Batch" + now.ToString("yyyy-MM-dd"));
		sheet.CreateRow(0).CreateCell(0).SetCellValue("Date");
		sheet.GetRow(0).CreateCell(1).SetCellValue("Name");
		sheet.GetRow(0).CreateCell(2).SetCellValue("Points");
		sheet.GetRow(0).CreateCell(3).SetCellValue("Timing");
		sheet.GetRow(0).CreateCell(4).SetCellValue("Tasks");
		FileStream fileStream3 = File.Create(path + excelName);
		((IWorkbook)hSSFWorkbook2).Write((Stream)fileStream3);
		fileStream3.Close();
	}

	public void UpdateExcel()
	{
		HSSFWorkbook hSSFWorkbook;
		using (FileStream fileStream = new FileStream(path + excelName, FileMode.Open, FileAccess.Read))
		{
			hSSFWorkbook = new HSSFWorkbook(fileStream);
			fileStream.Close();
		}
		ISheet sheetAt = hSSFWorkbook.GetSheetAt(0);
		sheetAt.CreateRow(sheetAt.LastRowNum + 1);
		excelScript.Details(sheetAt, tasks, TMP_Text.text, scorescript.Total, countdown, finished);
		using (FileStream fileStream2 = new FileStream(path + excelName, FileMode.Open, FileAccess.Write))
		{
			hSSFWorkbook.Write(fileStream2);
			fileStream2.Close();
		}
		StartCoroutine(Delay());
	}

	private IEnumerator Delay()
	{
		audioSource.PlayOneShot(teleportAnnouncement, 0.7f);
		Debug.Log("teleporting player, play ai voice to alert player");
		yield return new WaitForSeconds(4f);
		player.transform.position = new Vector3(classroomPosition.transform.position.x, classroomPosition.transform.position.y, classroomPosition.transform.position.z);
	}
}
