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