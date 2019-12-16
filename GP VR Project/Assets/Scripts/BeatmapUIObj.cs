using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public enum UIObjType {Song, Beatmap};

public class BeatmapUIObj : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	[Header("General Components")]
	public UIObjType uiObjType;
	public Animator anim; //Should have Component by default
	public BeatmapData bmd;
	public BeatmapInfo bmi;

	[Header ("For Displaying Splash Images")]
	//public Texture2D splashImg;
	public RawImage mainImg; //Should have Component by default

	[Header("For Playing Correct Audio")]
	public AudioClip audio;

	public void AssignBmData(BeatmapData bmd)
	{
		this.bmd = bmd;
		audio = bmd.audio;
		mainImg.texture = bmd.mainSplash;
	}

	public void AssignBmInfo(BeatmapInfo bmi)
	{
		this.bmi = bmi;
	}

	#region Direct Button Functions
	//For Song Select UI Objs
	public void OnSongSelect()
	{
		if (UIManager.isTransitioning) return;

		UIManager.isTransitioning = true;
		anim.SetBool("Clicked", true);
		anim.SetBool("Is Hovering", false);

		UIManager.inst.PopulateBeatmapSelect(bmd);
	}

	//For Beatmap Select UI Objs
	public void OnBeatmapSelect()
	{
		GameManager.inst.AssignMapData(bmd, bmi);
	}
	#endregion

	#region Animation Events
	public void ResetButtonAnimator()
	{
		anim.SetBool("Is Hovering", false);
		anim.SetBool("Clicked", false);
	}

	public void ShowBeatmaps()
	{
		UIManager.inst.OnSongSelect();
	}
	#endregion

	#region Pointer Events
	public void OnPointerEnter(PointerEventData eventData)
	{
		anim.SetBool("Is Hovering", true);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		anim.SetBool("Is Hovering", false);
	}
	#endregion
}
