using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Beatmap Object", menuName = "ScriptableObjects/New Beatmap Object", order = 1)]
public class BeatmapObject : ScriptableObject
{
	public string folderName;
	public string[] mapNames;
	public AudioClip audioFile;
}
