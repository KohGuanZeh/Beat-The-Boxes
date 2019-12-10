using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct BeatmapInfo
{
	[SerializeField] public string mapName;
	[SerializeField] public float difficulty;
}

[CreateAssetMenu(fileName = "Beatmap Object", menuName = "ScriptableObjects/New Beatmap Object", order = 1)]
public class BeatmapObject : ScriptableObject
{
	public string folderName;
	public BeatmapInfo[] mapInfos; 
	public AudioClip audioFile;

	public BeatmapObject(string folderName)
	{
		this.folderName = folderName;
	}
}
