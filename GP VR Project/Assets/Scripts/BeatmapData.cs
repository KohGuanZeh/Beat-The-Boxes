using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct BeatmapInfo
{
	[SerializeField] public string mapName;
	[SerializeField] public string creator;
	[SerializeField] public float difficulty;
	[SerializeField] public List<ScoreInfo> scores;
}

[System.Serializable]
public struct ScoreInfo
{
	[SerializeField] public Grade grade;
	[SerializeField] public int score;
	[SerializeField] public int maxCombo;
	[SerializeField] public int hits;
	[SerializeField] public int miss;
}

[System.Serializable]
public class BeatmapData
{
	[SerializeField] public string folderName;
	[SerializeField] public List<BeatmapInfo> mapInfos;
	[SerializeField] public string songName; //For UI Display Purposes
	[SerializeField] public string artistName; //For UI Display Purposes
	[SerializeField] public string audioFilePath;
	[SerializeField] public string imgFilePath;
	public AudioClip audio;
	public Texture2D mainSplash;

	public BeatmapData(string folderName = "")
	{
		this.folderName = folderName;
		mapInfos = new List<BeatmapInfo>();
	}

	public void LoadAssets()
	{
		audio = Resources.Load<AudioClip>(audioFilePath);
		mainSplash = Resources.Load<Texture2D>(imgFilePath);
	}

	public bool RemoveFromJson()
	{
		if (!audio) return true;
		//Debug.Log(System.IO.File.Exists(string.Format("{0}/Resources/Beatmaps/{1}/{2}.osu", Application.dataPath, folderName, mapInfos[0].mapName)));
		//Debug.Log(!System.IO.File.Exists(string.Format("{0}/Resources/Beatmaps/{1}/{2}.osu", Application.dataPath, folderName, mapInfos[0].mapName)));
		mapInfos.RemoveAll(item => !System.IO.File.Exists(string.Format("{0}/Resources/Beatmaps/{1}/{2}.osu", Application.dataPath, folderName, item.mapName)));
		if (mapInfos.Count == 0) return true;

		return false;
	}
}

[System.Serializable] //Needed since Json does not allow to Save List or Arrays by themselves
public class BeatmapDataWrapper
{
	public List<BeatmapData> beatmaps;

	public BeatmapDataWrapper(List<BeatmapData> beatmaps)
	{
		this.beatmaps = beatmaps;
	}
}