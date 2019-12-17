﻿using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

using OsuParsers.Beatmaps;
using OsuParsers.Beatmaps.Objects;
using OsuParsers.Beatmaps.Sections;
using OsuParsers.Decoders;

public enum GameState {Menu, Loading, OnPlay, Paused, Ended};

public enum Grade {S, A, B, C, D};

public class GameManager : MonoBehaviour
{
	[Header("General Components")]
	public static GameManager inst;
	[SerializeField] ObjectPooling objPooler;
	public AudioSource songPlayer;
	[SerializeField] const string beatMapFolder = @"/Resources/Beatmaps/";

	[Header("Load Beatmap via SO")]
	[SerializeField] BeatmapObject selectedMap;
	[SerializeField] int mapIndex;

	[Header("Load Beatmap via BMD and BMI")]
	[SerializeField] BeatmapData bmd;
	[SerializeField] BeatmapInfo bmi;

	[Header("Beatmap Infos")]
	[SerializeField] Beatmap beatmap;
	[SerializeField] List<HitObject> beats;

	[Header("Track Time")]
	[SerializeField] float trackDelayTime;
	[SerializeField] double trackStartTime;

	[Header("Beat Instantiation (Timing)")]
	[Range(10, 50)] public float scrollSpeed; //Controls how fast the Beats go towards the Player
	[SerializeField] float bufferTime; //Number of Seconds it takes for the Beat to reach the Player. Used to Check when it should Spawn Beats
	[SerializeField] float startTimeOffset; //Number of Seconds before the Song Plays

	[Header("Beat Instantiation (Transform Manipulation)")]
	[SerializeField] Beat[] beatPrefabs; //Sorted by Box Type. 0 is Normal, 1 is Slider, 2 is Directional
	[SerializeField] int colorIndex; //Know what Color of Beat to Spawn
	[SerializeField] Transform beatSpawnPos, beatHitPos;
	[SerializeField] float spawnHitDist;

	[Header("Beat Colors")]
	//All of which are according to the Color Index. Red, Blue, Green, Yellow
	public Color[] diffuseColors;
	public Color[] emissiveColors;
	public float[] emissiveIntensities;

	[Header("For Slider Configuration")]
	[Range(0.1f, 3f)] [SerializeField] float sliderThreshold = 0.5f; //How Long should the Beat Length be in order to consider Spawning Slider Beats
	[Range(0.01f, 0.5f)] [SerializeField] float sliderInterval = 0.2f; //How should the Slider Beats be Spread out. Eg. Every 0.2s spawn 1 Slider Beat

	[Header("For Scoring")]
	[SerializeField] int score;
	[SerializeField] int combo;

	[Header("Boxing Glove Mat")]
	public Material[] gloveMats; //Follow Box Color Index

	[Header("For Menus")]
	public GameState gameState;

	[Header("For Developers Use")]
	public bool autoMode;
	public bool generateSliderBeats;
	public bool playOnStart;
	public bool showStartDebug;
	public bool spawnOnlyOneBeat;
	public List<string> sliderInfo;

	private void Awake()
	{
		inst = this;
		beats = new List<HitObject>();
	}

	// Start is called before the first frame update
	void Start()
	{
		objPooler = ObjectPooling.inst;
		songPlayer = GetComponent<AudioSource>();
		if (playOnStart) InitialiseBeatMap(selectedMap, mapIndex);

		if (showStartDebug && beats.Count > 0) StartDebug(); 
	}

	// Update is called once per frame
	void Update()
	{
		if (gameState == GameState.OnPlay)
		{
			if (beats.Count > 0)
			{
				if (GetTrackTime() + bufferTime >= (double)beats[0].StartTime / 1000) SpawnBeat();
			}

			//if (!songPlayer.isPlaying && beats.Count == 0) OnSongEnd(); 
		}

		//For Pause
		if (Input.GetKeyDown(KeyCode.Escape)) PausePlayGame();

		//Acts as a Retry Button
		if (Input.GetKeyDown(KeyCode.Alpha0)) Retry();
	}

	#region Beat Map Initialisation
	public void AssignMapData(BeatmapData bmd, BeatmapInfo bmi)
	{
		this.bmd = bmd;
		this.bmi = bmi;
	}

	public void InitialiseBeatMap(BeatmapObject selectedMap, int mapIndex) //By Scriptable Object. Easier for Testing
	{
		this.selectedMap = selectedMap;
		this.mapIndex = mapIndex;

		if (mapIndex >= selectedMap.mapInfos.Length)
		{
			UnityEngine.Debug.LogError("Map Index Exceeds the Length of the Beat Map. Ensure that the Map Index is within the Array Length");
			return;
		}

		//SO Method
		string mapPath = GetMapPath(selectedMap.folderName, selectedMap.mapInfos[mapIndex].mapName);
		songPlayer.clip = selectedMap.audioFile;
		beatmap = BeatmapDecoder.Decode(mapPath);
		beats = beatmap.HitObjects;

		spawnHitDist = beatHitPos.transform.position.z - beatSpawnPos.transform.position.z;
		//beatMoveDir = spawnHitDist < 0 ? -1f : 1f;

		bufferTime = Mathf.Abs(spawnHitDist) / scrollSpeed;

		startTimeOffset = trackDelayTime;
		float firstBeatTime = (float)beats[0].StartTime / 1000;
		if (firstBeatTime < bufferTime) startTimeOffset += (bufferTime - firstBeatTime);

		trackStartTime = AudioSettings.dspTime + startTimeOffset;
		songPlayer.PlayScheduled(trackStartTime);

		gameState = GameState.OnPlay;
	}

	public void InitialiseBeatMap(/*BeatmapData bmd, BeatmapInfo bmi*/) //BMD to Load Clips, BMI to Load Map. Currently Using Variables instead of Parameters
	{
		string mapPath = GetMapPath(bmd.folderName, bmi.mapName);
		songPlayer.clip = bmd.audio;
		beatmap = BeatmapDecoder.Decode(mapPath);
		beats = beatmap.HitObjects;

		spawnHitDist = beatHitPos.transform.position.z - beatSpawnPos.transform.position.z;
		bufferTime = Mathf.Abs(spawnHitDist) / scrollSpeed;

		startTimeOffset = trackDelayTime;
		float firstBeatTime = (float)beats[0].StartTime / 1000;
		if (firstBeatTime < bufferTime) startTimeOffset += (bufferTime - firstBeatTime);

		trackStartTime = AudioSettings.dspTime + startTimeOffset;
		songPlayer.PlayScheduled(trackStartTime);

		gameState = GameState.OnPlay;
	}
	#endregion

	#region For Beat Spawning
	void SpawnBeat()
	{
		//What Beat to Spawn
		if (beats[0].IsNewCombo)
		{
			int prevColor = colorIndex;
			while (colorIndex == prevColor) colorIndex = UnityEngine.Random.Range(0, 4); //Randomise until you get a Different Color
		}

		//Check if Beat is a Slider
		double beatLength = beats[0].EndTime - beats[0].StartTime; //Get Total Time Span of Beat 

		if (beatLength/1000 >= sliderThreshold && generateSliderBeats) //If it Qualifies as a Slider
		{
			int beatsToSpawn = Mathf.FloorToInt((float)(beatLength/(sliderInterval * 1000))); //Check how many beats to spawn
			//print(string.Format("Function Calculates such that it is to spawn {0} Additional Beats. Beat Length is {1}, Slider Threshold is {2}", beatsToSpawn, beatLength, sliderInterval * 1000));
			double beatInterval = beatsToSpawn == 0 ? 0 : beatLength / beatsToSpawn;
			//print("Beat Interval is " + beatInterval);

			for (int i = 0; i <= beatsToSpawn; i++)
			{
				//Make Last Beat the Directional Beat
				bool isDirectional = i + 1 == beatsToSpawn;
				string tag = isDirectional ? "Directional" : "Slider";

				//Instantiate(beatPrefabs[1], beatSpawnPos.position, Quaternion.identity);
				Beat beat = objPooler.SpawnFromPool(tag, beatSpawnPos.position, Quaternion.identity, beatSpawnPos).GetComponent<Beat>(); 
				beat.InitialiseBeat(this, beatSpawnPos.position, spawnHitDist);

				//Set Slider Beats to their Correct Timing
				double additionalOffset = beatInterval * i;
				beat.SetBeatInstanceValues(GetBoxColorByIndex(colorIndex), GetTrackTime() + additionalOffset/1000, beats[0].StartTime + additionalOffset, isDirectional ? RandomiseValidBeatDirection() : Direction.None);
				beat.MoveBeatPosition();
			}
		}
		else
		{
			//Beat beat = Instantiate(beatPrefabs[i], beatSpawnPos.position, Quaternion.identity);
			Beat beat = objPooler.SpawnFromPool("Normal", beatSpawnPos.position, Quaternion.identity, beatSpawnPos).GetComponent<Beat>();
			beat.InitialiseBeat(this, beatSpawnPos.position, spawnHitDist);

			beat.SetBeatInstanceValues(GetBoxColorByIndex(colorIndex), GetTrackTime(), beats[0].StartTime);
			beat.MoveBeatPosition();
		}

		beats.RemoveAt(0);

		if (spawnOnlyOneBeat) beats.Clear();
	}

	//When the Song has ended, a Figure should run towards the Player. 
	//When it reaches the Player and the Player fails to land the first hit in time, the Game will just end
	//Player needs to hit a Specific part of the Figure (Single Beat).
	//If Player manages to land the first hit, 2 more hearbeat will spawn before Player lands the final Combo Punches.
	//Use Invoke Repeating for the Heatbeats and Combos for Accurate time (if needed)
	void OnSongEnd()
	{
		
	}
	#endregion

	#region For Track Time and Distance Between Spawn Pos and Hit Pos
	public float GetSpawnHitDist()
	{
		return spawnHitDist;
	}

	/// <summary>
	/// Gets the Current Track Time
	/// </summary>
	/// <returns>Returns the Song Time in Seconds</returns>
	public double GetTrackTime()
	{
		//Can Print Negative Values
		return AudioSettings.dspTime - trackStartTime;
	}
	#endregion

	#region For Scoring and Combo System
	public void AddScore(int score = 30)
	{
		score *= ++combo; //Add to Combo and Multiply it with the Score
		this.score += score;
	}

	public void SubtractScore(int penalty = 100)
	{
		BreakCombo();
		score = Mathf.Max(score - penalty, 0);
	}

	public void BreakCombo()
	{
		combo = 0;
	}
	#endregion

	#region For Menus
	public void PausePlayGame()
	{
		switch (gameState)
		{
			case GameState.OnPlay:
				gameState = GameState.Paused;
				Time.timeScale = 0;
				AudioListener.pause = true;
				break;
			case GameState.Paused:
				gameState = GameState.OnPlay;
				Time.timeScale = 1;
				AudioListener.pause = false;
				break;
		}
	}

	public void Retry()
	{
		//Check if the Game State is Paused
		if (gameState == GameState.Paused)
		{
			Time.timeScale = 1;
			AudioListener.pause = false;
		}

		//Find a Better Way to Find Objects Of Type
		foreach (Beat beat in FindObjectsOfType<Beat>()) objPooler.ReturnToPool(beat.gameObject, beat.GetPoolTag());
		gameState = GameState.Loading;

		songPlayer.Stop();
		InitialiseBeatMap(/*bmd, bmi*/);

		score = 0;
		combo = 0;
	}
	#endregion

	#region Modular Functions
	public static BoxColor GetBoxColorByIndex(int colorIndex)
	{
		switch (colorIndex)
		{
			case 0:
				return BoxColor.Red;
			case 1:
				return BoxColor.Blue;
			case 2:
				return BoxColor.Green;
			case 3:
				return BoxColor.Yellow;
			default:
				return BoxColor.None;
		}
	}

	public Direction RandomiseValidBeatDirection()
	{
		int i = Random.Range(1, 100);

		if (i <= 33) return Direction.Up;
		else if (i <= 66) return Direction.Down;
		else return Direction.Right; //Only need 1 as if its Left Lane, it must be hit Right. If its Right Lane, it must be hit Left.
	}
	#endregion

	#region Get File Paths
	string GetMapPath(string mapFolder, string mapName)
	{
		return string.Format("{0}{1}{2}/{3}.osu", Application.dataPath, beatMapFolder, mapFolder, mapName);
	}

	string GetAudioPath(string mapFolder, string songName)
	{
		return string.Format("{0}{1}{2}/{3}", Application.dataPath, beatMapFolder, mapFolder, songName);
	}
	#endregion

	#region For Debugging Purposes
	void StartDebug()
	{
		ColoursSection colours = beatmap.ColoursSection;
		for (int i = 0; i < colours.ComboColours.Count; i++)
		{
			print(colours.ComboColours[i]);
		}

		sliderInfo = new List<string>();
		for (int i = 0; i < beats.Count; i++)
		{
			if (beats[0].GetType() != typeof(Slider))
			{
				print(string.Format("Index {0} skipped!", i));
				continue;
			}

			string printString = string.Format("Index Slider = {0}. Start Time = {1}ms. End Time = {2}ms. Slider Length = {3}ms. Start Time Span = {4}ms. End Time Span = {5}ms. Total Time Span = {6}ms.",
			i, beats[i].StartTime, beats[i].EndTime, beats[i].EndTime - beats[i].StartTime, beats[i].StartTimeSpan.TotalMilliseconds, beats[i].EndTimeSpan.TotalMilliseconds, beats[i].TotalTimeSpan.TotalMilliseconds);

			sliderInfo.Add(printString);
			print(printString);
		}
	}
	#endregion
}
