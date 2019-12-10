using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

using OsuParsers.Beatmaps;
using OsuParsers.Beatmaps.Objects;
using OsuParsers.Beatmaps.Sections;
using OsuParsers.Decoders;

public enum GameState {Menu, Loading, OnPlay, Paused, Ended};

public class GameManager : MonoBehaviour
{
	[Header("General Components")]
	public static GameManager inst;
	[SerializeField] ObjectPooling objPooler;
	[SerializeField] AudioSource songPlayer;
	[SerializeField] const string beatMapFolder = @"/Resources/Beatmaps/";

	[Header("Beatmap Info Via SO")]
	[SerializeField] BeatmapObject selectedMap;
	[SerializeField] int mapIndex;

	[Header("Beatmap Infos")]
	[SerializeField] Beatmap beatmap;
	[SerializeField] List<HitObject> beats;

	[Header("Track Time")]
	[SerializeField] float trackDelayTime;
	[SerializeField] double trackStartTime;

	[Header("For Beat Instantiation")]
	[Range(10, 50)] public float scrollSpeed;
	[SerializeField] float bufferTime; //Number of Seconds 
	[SerializeField] float startTimeOffset; //Number of Seconds before the Song Plays
	[SerializeField] Beat[] beatPrefabs; //Colors are indicated in Beat.cs
	[SerializeField] int colorIndex;
	[SerializeField] Transform beatSpawnPos, beatHitPos;
	[SerializeField] float spawnHitDist;
	[SerializeField] Transform beatsParent;

	[Header("For Slider Configuration")]
	[Range(0.1f, 0.5f)] [SerializeField] float sliderThreshold;

	[Header("For Scoring")]
	[SerializeField] int score;
	[SerializeField] int combo;

	[Header("For Boxing")]
	[SerializeField] Renderer leftGloveR; 
	[SerializeField] Renderer rightGloveR;
	public BoxColor[] gloveColor; //0 Left, 1 Right
	public Material[] mats; //Follow Box Color Index

	[Header("For Menus")]
	public GameState gameState;

	[Header("For Developers Use")]
	public bool autoMode;
	public bool generateSliderBeats;
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
		//InitialiseBeatMap(selectedMap, mapIndex);

		if (showStartDebug)
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

			if (Input.GetKeyDown(KeyCode.X)) ChangeGloveColor(true);
			if (Input.GetKeyDown(KeyCode.N)) ChangeGloveColor(false);

			//if (!songPlayer.isPlaying && beats.Count == 0) OnSongEnd(); 
		}

		//For Pause
		if (Input.GetKeyDown(KeyCode.Escape)) PausePlayGame();

		//Acts as a Retry Button
		if (Input.GetKeyDown(KeyCode.Alpha0)) Retry();
	}

	void ChangeGloveColor(bool isLeft)
	{
		if (isLeft)
		{
			switch (gloveColor[0])
			{
				case BoxColor.Red:
					gloveColor[0] = BoxColor.Yellow;
					leftGloveR.material = mats[3];
					break;
				case BoxColor.Yellow:
					gloveColor[0] = BoxColor.Red;
					leftGloveR.material = mats[0];
					break;
			}
		}
		else
		{
			switch (gloveColor[1])
			{
				case BoxColor.Blue:
					gloveColor[1] = BoxColor.Green;
					rightGloveR.material = mats[2];
					break;
				case BoxColor.Green:
					gloveColor[1] = BoxColor.Blue;
					rightGloveR.material = mats[1];
					break;
			}
		}
	}

	#region Beat Map Initialisation
	public void InitialiseBeatMap(BeatmapObject selectedMap, int mapIndex)
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

		if (beatLength > 0 && generateSliderBeats) //If it Qualifies as a Slider
		{
			int beatsToSpawn = Mathf.FloorToInt((float)(beatLength/(sliderThreshold * 1000))); //Check how many beats to spawn
			//print(string.Format("Function Calculates such that it is to spawn {0} Additional Beats. Beat Length is {1}, Slider Threshold is {2}", beatsToSpawn, beatLength, sliderThreshold * 1000));
			double beatInterval = beatsToSpawn == 0 ? 0 : beatLength / beatsToSpawn;
			//print("Beat Interval is " + beatInterval);

			for (int i = 0; i <= beatsToSpawn; i++)
			{
				Beat beat = objPooler.SpawnFromPool(GetPoolTag(colorIndex), beatSpawnPos.position, Quaternion.identity).GetComponent<Beat>(); //Instantiate(beatPrefabs[colorIndex], beatSpawnPos.position, Quaternion.identity);

				beat.AssignGM(this);
				double additionalOffset = beatInterval * i;
				beat.InitialiseBeat(GetTrackTime() + additionalOffset/1000, beats[0].StartTime + additionalOffset, beatSpawnPos.position, spawnHitDist);
				beat.MoveBeatPosition();
				beat.transform.parent = beatsParent;
			}
		}
		else
		{
			Beat beat = objPooler.SpawnFromPool(GetPoolTag(colorIndex), beatSpawnPos.position, Quaternion.identity).GetComponent<Beat>();

			beat.AssignGM(this);
			beat.InitialiseBeat(GetTrackTime(), beats[0].StartTime, beatSpawnPos.position, spawnHitDist);
			beat.MoveBeatPosition();
			beat.transform.parent = beatsParent;
		}

		beats.RemoveAt(0);

		if (spawnOnlyOneBeat) beats.Clear();
	}

	string GetPoolTag(int colorIndex)
	{
		switch (colorIndex)
		{
			case 0:
				return "Red";
			case 1:
				return "Blue";
			case 2:
				return "Green";
			case 3:
				return "Yellow";
			default:
				return "None";
		}
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
		InitialiseBeatMap(selectedMap, mapIndex);
		score = 0;
		combo = 0;
	}
	#endregion

	#region GetFilePaths
	string GetMapPath(string mapFolder, string mapName)
	{
		return string.Format("{0}{1}{2}/{3}.osu", Application.dataPath, beatMapFolder, mapFolder, mapName);
	}

	string GetAudioPath(string mapFolder, string songName)
	{
		return string.Format("{0}{1}{2}/{3}", Application.dataPath, beatMapFolder, mapFolder, songName);
	}
	#endregion
}
