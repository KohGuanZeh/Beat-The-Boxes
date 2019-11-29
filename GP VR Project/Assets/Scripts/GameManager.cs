using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OsuParsers.Beatmaps;
using OsuParsers.Beatmaps.Objects;
using OsuParsers.Decoders;

public class GameManager : MonoBehaviour
{
	[Header("General Components")]
	[SerializeField] AudioSource songPlayer;
	[SerializeField] const string beatMapFolder = @"/Resources/Beatmaps/";

	[Header("Beatmap Info Via SO")]
	[SerializeField] BeatmapObject selectedMap;
	[SerializeField] int mapIndex;

	[Header("Beatmap Infos")]
	[SerializeField] Beatmap beatmap;
	[SerializeField] List<HitObject> beats;

	[Header("Track Time")]
	[SerializeField] double trackStartTime;

	[Header("For Beat Instantiation")]
	[Range(10, 30)] public float scrollSpeed;
	[SerializeField] float bufferTime; //Number of Seconds 
	[SerializeField] float startTimeOffset; //Number of Seconds before the Song Plays
	[SerializeField] Beat[] beatPrefabs; //Colors are indicated in Beat.cs
	[SerializeField] Transform beatSpawnPos, beatHitPos;
	[SerializeField] float spawnHitDist;
	[SerializeField] Transform beatsParent;
	[SerializeField] Stopwatch watch;

	[Header("For Scoring")]
	[SerializeField] int score;
	[SerializeField] int combo;
	[SerializeField] float myTime;

	[Header("For Developers Use")]
	public bool autoMode;
	bool spawned;

	// Start is called before the first frame update
	void Start()
	{
		songPlayer = GetComponent<AudioSource>();
		
		//SO Method
		string mapPath = GetMapPath(selectedMap.folderName, selectedMap.mapNames[mapIndex]);
		songPlayer.clip = selectedMap.audioFile;
		beatmap = BeatmapDecoder.Decode(mapPath);
		beats = beatmap.HitObjects;

		spawnHitDist = beatHitPos.transform.position.z - beatSpawnPos.transform.position.z;
		//beatMoveDir = spawnHitDist < 0 ? -1f : 1f;

		bufferTime = Mathf.Abs(spawnHitDist) / scrollSpeed;

		startTimeOffset = 1.5f;
		float firstBeatTime = (float)beats[0].StartTime / 1000;
		if (firstBeatTime < bufferTime) startTimeOffset += (bufferTime - firstBeatTime);

		trackStartTime = AudioSettings.dspTime + startTimeOffset;
		songPlayer.PlayScheduled(trackStartTime);
		print(trackStartTime);
	}

	// Update is called once per frame
	void Update()
	{
		if (beats.Count > 0)
		{
			if (GetTrackTime() + bufferTime >= (double)beats[0].StartTime / 1000) SpawnBeat();
		}

		if (!songPlayer.isPlaying && beats.Count == 0) OnSongEnd(); 
	}

	#region For Beat Spawning
	void SpawnBeat()
	{
		//What Beat to Spawn
		int i = UnityEngine.Random.Range(0,4);
		if (beats[0].GetType() == typeof(Slider) && beats[0].TotalTimeSpan.TotalSeconds > 1) i = 5;
		Beat beat = Instantiate(beatPrefabs[i], beatSpawnPos.position, Quaternion.identity);

		beat.AssignGM(this);
		beat.InitialiseBeat(GetTrackTime(), beats[0].StartTime, beats[0].StartTime, beatSpawnPos.position, spawnHitDist);
		beat.MoveBeatPosition();
		beat.transform.parent = beatsParent;

		beats.RemoveAt(0);
	}

	//When the Song has ended, a Figure should run towards the Player. 
	//When it reaches the Player and the Player fails to land the first hit in time, the Game will just end
	//Player needs to hit a Specific part of the Figure (Single Beat).
	//If Player manages to land the first hit, 2 more hearbeat will spawn before Player lands the final Combo Punches.
	//Use Invoke Repeating for the Heatbeats and Combos for Accurate time (if needed)
	void OnSongEnd()
	{
		string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
		UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
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
