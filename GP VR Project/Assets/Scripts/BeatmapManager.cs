using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

public static class BeatmapManager
{
	public static List<BeatmapData> beatmaps;

	public static List<BeatmapData> LoadBeatmapData()
	{
		string jsonPath = string.Format("{0}/{1}/{2}", Application.dataPath, "Beatmaps", "MapData.txt");

		if (File.Exists(jsonPath))
		{
			string json = File.ReadAllText(jsonPath);
			BeatmapDataWrapper bmWrapper = JsonUtility.FromJson<BeatmapDataWrapper>(json);
			return bmWrapper.beatmaps;
		}
		else return new List<BeatmapData>();
	}

	public static void SaveBeatmapData()
	{
		string jsonPath = string.Format("{0}/{1}/{2}", Application.dataPath, "Beatmaps", "MapData.txt");

		beatmaps.Sort((x, y) => x.folderName.CompareTo(y.folderName));
		BeatmapDataWrapper bmWrapper = new BeatmapDataWrapper(beatmaps); //Create a Temp Wrapper so it can be stored as Json

		string json = JsonUtility.ToJson(bmWrapper, true);
		File.WriteAllText(jsonPath, json);
	}
}
