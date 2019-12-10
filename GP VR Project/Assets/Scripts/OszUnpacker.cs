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

	[Header("BMOs and BMDs")]
	[SerializeField] const string osuDumpPath = @"/Resources/Beatmaps/";
	[SerializeField] const string bmObjDirPath = @"/Scriptable Objects/Beatmap Objects/";
	[SerializeField] List<BeatmapObject> bmos;
	[SerializeField] List<BeatmapData> bmds;

	[Header("For Osz Unpacking")]
	[SerializeField] DirectoryInfo streamingAssetsDirInfo;
	[SerializeField] string tempPath; //Cache Temp Path as it will always be used to Dump all Files from Osz first when Extracting
	[SerializeField] bool unpackingInProgress;

	/*public delegate void VoidDelegate();
	public VoidDelegate OnUnpackedEnded;*/

	// Start is called before the first frame update
	void Start()
    {
		BeatmapManager.beatmaps = BeatmapManager.LoadBeatmapData();

		tempPath = Application.dataPath + osuDumpPath + "Temp/";
		if (!Directory.Exists(tempPath)) Directory.CreateDirectory(tempPath); //Ensure that the Temp Directory Exist to dump files

		#region For BMO to BMD Conversion
		/*if (initialiseBmos)
		{
			foreach (BeatmapObject bmo in bmos)
			{
				BeatmapData bmd = new BeatmapData(bmo.folderName);
				bmd.mapInfos = bmo.mapInfos;
				bmd.audioFile = bmo.audioFile;
				BeatmapManager.beatmaps.Add(bmd);
			}

			BeatmapManager.SaveBeatMapData();
		}*/
		#endregion

		streamingAssetsDirInfo = new DirectoryInfo(Application.streamingAssetsPath);
		FileInfo[] oszFiles = GetNewOszFiles();
		if (oszFiles.Length > 0) unpackingInProgress = false;

		foreach (FileInfo oszFile in oszFiles) UnpackOszFile(oszFile);

		unpackingInProgress = false;
		print("Unpacking Completed");

		BeatmapManager.SaveBeatmapData();
		bmds = BeatmapManager.beatmaps;

		//if (OnUnpackedEnded != null) OnUnpackedEnded();
	}

    // Update is called once per frame
    void Update()
    {

    }

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
				bmd = BeatmapManager.beatmaps.FirstOrDefault(x => x.folderName == folderName);

				if (bmd == null) bmd = new BeatmapData(); //bmdExists remains false
				else bmdExists = true;

				bmd.folderName = folderName;

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
				difficulty = beatmap.DifficultySection.OverallDifficulty
			};

			//Check for any missing Beatmap Info
			if (!bmdExists || !bmd.mapInfos.Exists(x => x.mapName == bmi.mapName)) bmd.mapInfos.Add(bmi);
			if (!File.Exists(string.Format("{0}{1}{2}", dumpPath, bmi.mapName, ".osu"))) File.Move(allMapPaths[i], string.Format("{0}{1}{2}", dumpPath, bmi.mapName, ".osu"));
		}

		bmd.mapInfos.Sort((x, y) => x.difficulty.CompareTo(y.difficulty));
		if (!bmdExists) BeatmapManager.beatmaps.Add(bmd);

		/*DirectoryInfo dir = new DirectoryInfo(tempPath);
		foreach (FileInfo file in dir.GetFiles()) File.Delete(file.FullName);*/

		Directory.Delete(tempPath, true);
		File.Delete(oszFile.FullName);
	}

	FileInfo[] GetNewOszFiles()
	{
		return streamingAssetsDirInfo.GetFiles("*.osz");
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
}
