using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public enum UIObjType {Song, Beatmap};

public class BeatmapUIObj : MonoBehaviour
{
	[Header("General Components")]
	public UIObjType uiObjType;
	public Animator anim; //Should have Component by default
	public BeatmapData bmd;
	public BeatmapInfo bmi;

	[Header ("For Displaying Splash Images")]
	public Texture2D splashImg;
	public RawImage mainImg; //Should have Component by default

	[Header("For Playing Correct Audio")]
	public AudioClip audio;

	public void AssignBmData(BeatmapData bmd)
	{
		this.bmd = bmd;
		audio = bmd.audio;
		splashImg = bmd.mainSplash;

		mainImg.texture = splashImg;
	}

	public void AssignBmInfo(BeatmapInfo bmi)
	{
		this.bmi = bmi;
	}

	//For Song Select UI Objs
	public void ShowBeatmaps()
	{

	}

	//For Beatmap Select UI Objs
	public void PlayMap()
	{
		GameManager.inst.TempPlay();
	}

}
