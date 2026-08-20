using System.Collections.Generic;
using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using TMPro;
using UnityEngine;

public class retrieveData : MonoBehaviour
{
	public List<ICell> scoreboardList = new List<ICell>();

	public TMP_Text firstName;

	public TMP_Text firstTime;

	public TMP_Text firstScore;

	public TMP_Text firstTask;

	public TMP_Text secondName;

	public TMP_Text secondTime;

	public TMP_Text secondScore;

	public TMP_Text secondTask;

	public TMP_Text thirdName;

	public TMP_Text thirdTime;

	public TMP_Text thirdScore;

	public TMP_Text thirdTask;

	public TMP_Text fourthName;

	public TMP_Text fourthTime;

	public TMP_Text fourthScore;

	public TMP_Text fourthTask;

	public TMP_Text fifthName;

	public TMP_Text fifthTime;

	public TMP_Text fifthScore;

	public TMP_Text fifthTask;

	private string excelName;

	private string path;

	private void Start()
	{
		path = SavePaths.OutputDirectory + "/";
		if (TimerSettings.setTime == 180f)
		{
			excelName = "RSAF_VIP_Leaderboard.xls";
		}
		else if (TimerSettings.setTime == 600f)
		{
			excelName = "RSAF_Leaderboard.xls";
		}
		if (!File.Exists(path + excelName))
		{
			return;
		}
		HSSFWorkbook hSSFWorkbook;
		using (FileStream fileStream = new FileStream(path + excelName, FileMode.Open, FileAccess.Read))
		{
			hSSFWorkbook = new HSSFWorkbook(fileStream);
			fileStream.Close();
		}
		ISheet sheetAt = hSSFWorkbook.GetSheetAt(0);
		for (int i = 1; i < 6; i++)
		{
			if (sheetAt.GetRow(i) != null)
			{
				for (int j = 1; j < 17; j++)
				{
					if (sheetAt.GetRow(i).GetCell(j, MissingCellPolicy.RETURN_NULL_AND_BLANK) != null)
					{
						scoreboardList.Add(sheetAt.GetRow(i).GetCell(j));
					}
				}
			}
			else if (sheetAt.GetRow(i) == null)
			{
				break;
			}
			switch (i)
			{
			case 1:
				firstName.text = scoreboardList[0].StringCellValue;
				firstScore.text = scoreboardList[1].NumericCellValue.ToString() ?? "";
				firstTime.text = scoreboardList[2].StringCellValue;
				firstTask.text = scoreboardList[3].StringCellValue;
				break;
			case 2:
				secondName.text = scoreboardList[0].StringCellValue;
				secondScore.text = scoreboardList[1].NumericCellValue.ToString() ?? "";
				secondTime.text = scoreboardList[2].StringCellValue;
				secondTask.text = scoreboardList[3].StringCellValue;
				break;
			case 3:
				thirdName.text = scoreboardList[0].StringCellValue;
				thirdScore.text = scoreboardList[1].NumericCellValue.ToString() ?? "";
				thirdTime.text = scoreboardList[2].StringCellValue;
				thirdTask.text = scoreboardList[3].StringCellValue;
				break;
			case 4:
				fourthName.text = scoreboardList[0].StringCellValue;
				fourthScore.text = scoreboardList[1].NumericCellValue.ToString() ?? "";
				fourthTime.text = scoreboardList[2].StringCellValue;
				fourthTask.text = scoreboardList[3].StringCellValue;
				break;
			case 5:
				fifthName.text = scoreboardList[0].StringCellValue;
				fifthScore.text = scoreboardList[1].NumericCellValue.ToString() ?? "";
				fifthTime.text = scoreboardList[2].StringCellValue;
				fifthTask.text = scoreboardList[3].StringCellValue;
				break;
			}
			scoreboardList.Clear();
		}
		using FileStream fileStream2 = new FileStream(path + excelName, FileMode.Open, FileAccess.Write);
		hSSFWorkbook.Write(fileStream2);
		fileStream2.Close();
	}
}
