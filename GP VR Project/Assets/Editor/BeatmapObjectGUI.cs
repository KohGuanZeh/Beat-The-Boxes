using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using OsuParsers.Beatmaps;
using OsuParsers.Beatmaps.Objects;
using OsuParsers.Beatmaps.Sections;
using OsuParsers.Decoders;

[CustomEditor(typeof(BeatmapObject))]
[CanEditMultipleObjects]
public class BeatmapObjectGUI : Editor
{
	const string beatMapFolder = @"/Resources/Beatmaps/";
	BeatmapObject mapObj;

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		mapObj = (BeatmapObject)target;

		GUILayout.Space(10);
		if (GUILayout.Button("Initialise Beatmap Object")) InitialiseBeatmapObject();
		//if (GUILayout.Button("Arrange Map Names By Difficulty")) ArrangeBeatMapViaDifficulty();
	}

	//Still need to manually add Beatmap Folder Name, and manually add Audio Clip file
	public void InitialiseBeatmapObject()
	{
		string[] mapNames = GetAllMapNames();
		mapObj.mapInfos = new BeatmapInfo[mapNames.Length];
		for (int i = 0; i < mapObj.mapInfos.Length; i++) mapObj.mapInfos[i].mapName = mapNames[i];

		ArrangeBeatMapViaDifficulty();
	}

	public void ArrangeBeatMapViaDifficulty()
	{
		//Get Maps Difficulty First
		for (int i = 0; i < mapObj.mapInfos.Length; i++)
		{
			string mapPath = GetMapPath(mapObj.folderName, mapObj.mapInfos[i].mapName);
			Beatmap beatmap = BeatmapDecoder.Decode(mapPath);
			mapObj.mapInfos[i].difficulty = beatmap.DifficultySection.OverallDifficulty;
		}

		System.Array.Sort(mapObj.mapInfos, (x, y) => x.difficulty.CompareTo(y.difficulty));
	}

	string[] GetAllMapNames()
	{
		string dir = GetMapFolderPath(mapObj.folderName);

		if (!Directory.Exists(dir))
		{
			Debug.LogError("Directory does not Exist. Please input a valid Folder Name and ensure that is is placed under " + beatMapFolder);
			return new string[0];
		}

		string[] matchedFiles = Directory.GetFiles(dir).Where(s => s.EndsWith(".osu")).ToArray();
		for (int i = 0; i < matchedFiles.Length; i++) matchedFiles[i] = Path.GetFileNameWithoutExtension(matchedFiles[i]);

		return matchedFiles;
	}

	string GetMapFolderPath(string mapFolder)
	{
		return string.Format("{0}{1}{2}", Application.dataPath, beatMapFolder, mapFolder);
	}

	string GetMapPath(string mapFolder, string mapName)
	{
		return string.Format("{0}{1}{2}/{3}.osu", Application.dataPath, beatMapFolder, mapFolder, mapName);
	}
}
