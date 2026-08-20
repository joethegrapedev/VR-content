using System;
using System.IO;
using NPOI.SS.UserModel;
using UnityEngine;

public class Leaderboard : MonoBehaviour
{
	private string path;

	public void Details(ISheet sheet, int tasks, string timeString, int score, float countdown, bool finished)
	{
		path = SavePaths.PlayerInfoFile;
		DateTime now = DateTime.Now;
		for (int i = 1; i < sheet.LastRowNum + 1; i++)
		{
			IRow row = sheet.GetRow(i);
			if (finished)
			{
				if (row.GetCell(3) != null)
				{
					double numericCellValue = row.GetCell(2).NumericCellValue;
					if ((double)score > numericCellValue)
					{
						sheet.ShiftRows(i, sheet.LastRowNum, 1);
						sheet.RemoveRow(sheet.GetRow(sheet.LastRowNum));
						row.CreateCell(0).SetCellValue(now.ToString("yyyy-MM-dd"));
						row.CreateCell(1).SetCellValue(JsonUtility.FromJson<PlayerName>(File.ReadAllText(path)).Player);
						row.CreateCell(2).SetCellValue(score);
						row.CreateCell(3).SetCellValue(timeString);
						row.CreateCell(4).SetCellValue(tasks + "/13");
						break;
					}
					if ((double)score == numericCellValue && row.GetCell(3).StringCellValue != "DNF")
					{
						string[] array = row.GetCell(3).ToString().Split(':');
						float num = float.Parse(array[0]) * 60f;
						float num2 = float.Parse(array[1]);
						float num3 = num + num2;
						if (countdown < num3)
						{
							sheet.ShiftRows(i, sheet.LastRowNum, 1);
							sheet.RemoveRow(sheet.GetRow(sheet.LastRowNum));
							row.CreateCell(0).SetCellValue(now.ToString("yyyy-MM-dd"));
							row.CreateCell(1).SetCellValue(JsonUtility.FromJson<PlayerName>(File.ReadAllText(path)).Player);
							row.CreateCell(2).SetCellValue(score);
							row.CreateCell(3).SetCellValue(timeString);
							row.CreateCell(4).SetCellValue(tasks + "/13");
							break;
						}
					}
					else if (row.GetCell(3).StringCellValue == "DNF")
					{
						sheet.ShiftRows(i, sheet.LastRowNum, 1);
						sheet.RemoveRow(sheet.GetRow(sheet.LastRowNum));
						row.CreateCell(0).SetCellValue(now.ToString("yyyy-MM-dd"));
						row.CreateCell(1).SetCellValue(JsonUtility.FromJson<PlayerName>(File.ReadAllText(path)).Player);
						row.CreateCell(2).SetCellValue(score);
						row.CreateCell(3).SetCellValue(timeString);
						row.CreateCell(4).SetCellValue(tasks + "/13");
						break;
					}
				}
				else if (row.GetCell(3) == null)
				{
					row.CreateCell(0).SetCellValue(now.ToString("yyyy-MM-dd"));
					row.CreateCell(1).SetCellValue(JsonUtility.FromJson<PlayerName>(File.ReadAllText(path)).Player);
					row.CreateCell(2).SetCellValue(score);
					row.CreateCell(3).SetCellValue(timeString);
					row.CreateCell(4).SetCellValue(tasks + "/13");
					break;
				}
			}
			if (finished)
			{
				continue;
			}
			if (row.GetCell(3) == null)
			{
				row.CreateCell(0).SetCellValue(now.ToString("yyyy-MM-dd"));
				row.CreateCell(1).SetCellValue(JsonUtility.FromJson<PlayerName>(File.ReadAllText(path)).Player);
				row.CreateCell(2).SetCellValue(score);
				row.CreateCell(3).SetCellValue("DNF");
				row.CreateCell(4).SetCellValue(tasks + "/13");
				break;
			}
			if (row.GetCell(3).StringCellValue == "DNF")
			{
				double numericCellValue2 = row.GetCell(2).NumericCellValue;
				if ((double)score > numericCellValue2 || (double)score == numericCellValue2)
				{
					sheet.ShiftRows(i, sheet.LastRowNum, 1);
					sheet.RemoveRow(sheet.GetRow(sheet.LastRowNum));
					row.CreateCell(0).SetCellValue(now.ToString("yyyy-MM-dd"));
					row.CreateCell(1).SetCellValue(JsonUtility.FromJson<PlayerName>(File.ReadAllText(path)).Player);
					row.CreateCell(2).SetCellValue(score);
					row.CreateCell(3).SetCellValue("DNF");
					row.CreateCell(4).SetCellValue(tasks + "/13");
					break;
				}
			}
		}
		int num4 = 0;
		path = SavePaths.PlayerInfoFile;
		for (int j = 1; j < sheet.LastRowNum + 1; j++)
		{
			IRow row2 = sheet.GetRow(j);
			if (row2.GetCell(1, MissingCellPolicy.RETURN_NULL_AND_BLANK) == null)
			{
				continue;
			}
			string text = row2.GetCell(1).ToString();
			if (!(JsonUtility.FromJson<PlayerName>(File.ReadAllText(path)).Player == text))
			{
				continue;
			}
			num4++;
			Debug.Log(num4);
			if (num4 == 2)
			{
				sheet.RemoveRow(row2);
				if (sheet.LastRowNum + 1 != j)
				{
					sheet.ShiftRows(j + 1, sheet.LastRowNum, -1);
				}
				break;
			}
		}
	}
}
