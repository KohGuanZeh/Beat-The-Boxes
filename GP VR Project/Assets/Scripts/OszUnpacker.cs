using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

using UnityEngine;

using OsuParsers.Beatmaps;
using OsuParsers.Beatmaps.Objects;
using OsuParsers.Beatmaps.Sections;
using OsuParsers.Decoders;

public class OszUnpacker : MonoBehaviour
{
	[Header("Developer Options")]
	[SerializeField] bool initialiseBmos;

	[Header("Beatmap Data")]
	[SerializeField] const string osuDumpPath = @"/Resources/Beatmaps/";
	[SerializeField] const string bmObjDirPath = @"/Scriptable Objects/Beatmap Objects/";
	public static List<BeatmapData> bmds; //For Other Classes to Access this BMD

	[Header("For Osz Unpacking")]
	public static bool unpackingInProgress;
	[SerializeField] DirectoryInfo streamingAssetsDirInfo;
	[SerializeField] string tempPath; //Cache Temp Path as it will always be used to Dump all Files from Osz first when Extracting
	[SerializeField] bool runtimeUnpacking;
	[SerializeField] GameObject overlayCanvas;
	[SerializeField] BeatmapData lastAddedBmd;

	/*public delegate void VoidDelegate();
	public VoidDelegate OnUnpackedEnded;*/

	// Start is called before the first frame update
	void Awake()
    {
		bmds = LoadBeatmapData();

		tempPath = Application.dataPath + osuDumpPath + "Temp/";
		if (!Directory.Exists(tempPath)) Directory.CreateDirectory(tempPath); //Ensure that the Temp Directory Exist to dump files

		streamingAssetsDirInfo = new DirectoryInfo(Application.streamingAssetsPath);
		FileInfo[] oszFiles = GetNewOszFiles();
		if (oszFiles.Length > 0) unpackingInProgress = true;

		foreach (FileInfo oszFile in oszFiles) UnpackOszFile(oszFile);

		unpackingInProgress = false;
		print("Unpacking Completed");

		SaveBeatmapData(bmds);

		InvokeRepeating("UnpackOszInRunTime", 5, 5);
	}

	public void UnpackOszInRunTime()
	{
		if (unpackingInProgress || GameManager.inst.gameState != GameState.Menu || !UIManager.inst.allowRuntimeUnpack) return;

		FileInfo[] oszFiles = GetNewOszFiles();
		if (oszFiles.Length > 0)
		{
			unpackingInProgress = true;
			runtimeUnpacking = true;
			overlayCanvas.gameObject.SetActive(true);
		}
		else return;

		StartCoroutine(UnpackOszFiles(oszFiles));
	}

	IEnumerator UnpackOszFiles(FileInfo[] oszFiles)
	{
		foreach (FileInfo oszFile in oszFiles) UnpackOszFile(oszFile);

		runtimeUnpacking = false;
		unpackingInProgress = false;
		SaveBeatmapData(bmds);
		overlayCanvas.gameObject.SetActive(false);

		print("Loading File Assets");

		if (UIManager.inst.selectPhase == 1)
		{
			UIManager.inst.ignoreSavedScrollPos = true;
			UIManager.inst.BackToSongSelect();
		} 
		UIManager.inst.PopulateSongSelect();
		//print(bmds.IndexOf(bmds.Where(item => item.folderName == lastAddedBmd.folderName).FirstOrDefault()));
		UIManager.inst.SelectSpecificSong(bmds.IndexOf(bmds.Where(item => item.folderName == lastAddedBmd.folderName).FirstOrDefault()));

		yield return null;
	}

	#region For Saving and Loading Beatmap Data
	public static List<BeatmapData> LoadBeatmapData()
	{
		string jsonPath = string.Format("{0}/{1}", Application.streamingAssetsPath, "MapData.txt");

		if (File.Exists(jsonPath))
		{
			string json = File.ReadAllText(jsonPath);
			BeatmapDataWrapper bmWrapper = JsonUtility.FromJson<BeatmapDataWrapper>(json);
			foreach (BeatmapData bmd in bmWrapper.beatmaps) bmd.LoadAssets();
			bmWrapper.beatmaps.RemoveAll(item => item.RemoveFromJson());
			return bmWrapper.beatmaps;
		}
		else return new List<BeatmapData>();
	}

	public static void SaveBeatmapData(List<BeatmapData> beatmaps)
	{
		string jsonPath = string.Format("{0}/{1}", Application.streamingAssetsPath, "MapData.txt");

		beatmaps.Sort((x, y) => x.folderName.CompareTo(y.folderName));
		BeatmapDataWrapper bmWrapper = new BeatmapDataWrapper(beatmaps); //Create a Temp Wrapper so it can be stored as Json

		string json = JsonUtility.ToJson(bmWrapper, true);
		File.WriteAllText(jsonPath, json);
	}
	#endregion

	void UnpackOszFile(FileInfo oszFile)
	{
		//First, ensure Temp Path Directory is Created. This is where all the Contents of the Osz Files will be dumped before it is moved to the official folder
		//tempPath = Application.dataPath + osuDumpPath + "Temp/"; //Set Correct Path to Temp Folder
		if (!Directory.Exists(tempPath)) Directory.CreateDirectory(tempPath);
		ZipFile.ExtractToDirectory(oszFile.FullName, tempPath);

		string[] allMapPaths = GetAllMapPaths(tempPath); //Gets all Osu files per Osz dumped in the Temp Path

		//Create Variables that will Store the Beatmap Data that will eventually save it to a Json File
		string folderName = string.Empty; //Meant to Store the Name for the Folder
		string dumpPath = string.Empty; //The Folder to Dump the Files
		BeatmapData bmd = null; //Create a Beatmap Data Class that will be initialised on the first For Loop

		bool bmdExists = false; //To Check if the Existing BMD exists

		for (int i = 0; i < allMapPaths.Length; i++)
		{
			//Decode the Beatmap
			Beatmap beatmap = BeatmapDecoder.Decode(allMapPaths[i]);

			if (i == 0) //If this is the First Map that is Decoded
			{
				//Initialise the Folder Name and the Dump Path for the Beatmap
				folderName = string.Format("{0} - {1}", beatmap.MetadataSection.Title, beatmap.MetadataSection.BeatmapSetID); //Initialise the Folder Name
				dumpPath = string.Format("{0}{1}{2}/", Application.dataPath, osuDumpPath, folderName); //Set Path to Dump ONLY the required resources for this Game

				//Check if the a BMD has been Created for the Osz Before
				bmd = bmds.FirstOrDefault(x => x.folderName == folderName);

				if (bmd == null) bmd = new BeatmapData(); //bmdExists remains false
				else bmdExists = true;

				bmd.folderName = folderName;
				bmd.songName = beatmap.MetadataSection.Title;
				bmd.artistName = beatmap.MetadataSection.Artist;

				if (!Directory.Exists(dumpPath)) Directory.CreateDirectory(dumpPath);

				//Only Get Audio File if it is missing
				if (string.IsNullOrEmpty(bmd.audioFilePath))
				{
					string audioName = beatmap.GeneralSection.AudioFilename;
					string destinationPath = string.Format("{0}{1}", dumpPath, audioName);
					
					//Only Move Audio File if it does not Exist
					if (!File.Exists(destinationPath)) File.Move(tempPath + audioName, destinationPath);
					audioName = GetFileNameNoExt(destinationPath); //Update Audio Name to have no Exts for Resources.Load

					bmd.audioFilePath = string.Format("Beatmaps/{0}/{1}", folderName, audioName); //Path is Designed for Resources.Load
				}

				if (string.IsNullOrEmpty(bmd.imgFilePath))
				{
					string imgName = beatmap.EventsSection.BackgroundImage;
					string destinationPath = string.Format("{0}{1}", dumpPath, imgName);

					//Only Move Img File if it does not Exist
					if (!File.Exists(destinationPath)) File.Move(tempPath + imgName, destinationPath);
					imgName = GetFileNameNoExt(destinationPath); //Update Image Name to have no Exts for Resources.Load

					bmd.imgFilePath = string.Format("Beatmaps/{0}/{1}", folderName, imgName); //Path is Designed for Resources.Load
				}
			}

			BeatmapInfo bmi = new BeatmapInfo
			{
				mapName = GetFileNameNoExt(allMapPaths[i]),
				creator = beatmap.MetadataSection.Creator,
				difficulty = beatmap.DifficultySection.OverallDifficulty
			};

			bmi.scores = new List<ScoreInfo>();

			//Check for any missing Beatmap Info
			if (!bmdExists || !bmd.mapInfos.Exists(x => x.mapName == bmi.mapName)) bmd.mapInfos.Add(bmi);
			if (!File.Exists(string.Format("{0}{1}{2}", dumpPath, bmi.mapName, ".osu"))) File.Move(allMapPaths[i], string.Format("{0}{1}{2}", dumpPath, bmi.mapName, ".osu"));
		}

		bmd.mapInfos.Sort((x, y) => x.difficulty.CompareTo(y.difficulty));
		bmd.LoadAssets();
		if (!bmdExists) bmds.Add(bmd);

		if (runtimeUnpacking) lastAddedBmd = bmds.Last();

		/*DirectoryInfo dir = new DirectoryInfo(tempPath);
		foreach (FileInfo file in dir.GetFiles()) File.Delete(file.FullName);*/

		Directory.Delete(tempPath, true);
		File.Delete(oszFile.FullName);

		print("Unpacking File Completed");
	}

	#region For Getting File Infos and File Names
	FileInfo[] GetNewOszFiles()
	{
		return streamingAssetsDirInfo.GetFiles("*.osz", SearchOption.TopDirectoryOnly);
	}

	string GetFileNameNoExt(string filePath)
	{
		if (File.Exists(filePath)) return Path.GetFileNameWithoutExtension(filePath);
		else return string.Empty;
	}

	string[] GetAllMapPaths(string dumpPath)
	{
		if (!Directory.Exists(dumpPath))
		{
			Debug.LogError(dumpPath + " has not been created. Unable to initialise further");
			return new string[0];
		}

		return Directory.GetFiles(dumpPath).Where(s => s.EndsWith(".osu")).ToArray();
	}

	string[] GetAllMapNames(string dumpPath)
	{
		if (!Directory.Exists(dumpPath))
		{
			Debug.LogError(dumpPath + " has not been created. Unable to initialise further");
			return new string[0];
		}

		string[] matchedFiles = Directory.GetFiles(dumpPath).Where(s => s.EndsWith(".osu")).ToArray();
		for (int i = 0; i < matchedFiles.Length; i++) matchedFiles[i] = Path.GetFileNameWithoutExtension(matchedFiles[i]);

		return matchedFiles;
	}
	#endregion
}
