using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BeatmapData
{
	[SerializeField] public string folderName;
	[SerializeField] public List<BeatmapInfo> mapInfos;
	[SerializeField] public string audioFilePath;
	[SerializeField] public string imgFilePath;

	public BeatmapData(string folderName = "")
	{
		this.folderName = folderName;
		mapInfos = new List<BeatmapInfo>();
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