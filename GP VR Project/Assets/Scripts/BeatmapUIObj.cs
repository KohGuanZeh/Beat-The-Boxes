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
	public bool isHovering;
	public bool checkIfIsScrolling;
	public float checkTime;
	public float hoverTime;

	[Header("For Playing Correct Audio")]
	public AudioClip audio;

	void Update()
	{
		anim.SetFloat("Normalized Time", Mathf.PingPong(Time.time, 1));
		SelectOnHover();
	}

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
		if (UIManager.isTransitioning || !UIManager.inst.CanTriggerScrollButtons()) return;

		UIManager.isTransitioning = true;
		anim.SetBool("Clicked", true);
		anim.SetBool("Is Hovering", false);

		UIManager.inst.PopulateBeatmapSelect(bmd);
		UIManager.inst.OnTransitionEnd += ResetButtonAnimator;
		UIManager.inst.OnHoverSelect(this); //Display the Thing on Big Screen
	}

	//For Beatmap Select UI Objs
	public void OnBeatmapSelect()
	{
		GameManager.inst.AssignMapData(bmd, bmi);
	}
	#endregion

	#region Animation Events
	void ResetButtonAnimator()
	{
		anim.SetBool("Is Hovering", false);
		anim.SetBool("Clicked", false);
		UIManager.inst.OnTransitionEnd -= ResetButtonAnimator; //Called Through Delegate so that it can be handled on UIManager End instead
	}

	public void ShowBeatmaps()
	{
		UIManager.inst.OnSongSelect();
	}
	#endregion

	#region Pointer Events
	public void OnPointerEnter(PointerEventData eventData)
	{
		isHovering = true;
		anim.SetBool("Is Hovering", true);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		isHovering = false;
		anim.SetBool("Is Hovering", false);
	}

	void SelectOnHover()
	{
		if (isHovering && !UIManager.isTransitioning)
		{
			hoverTime = Mathf.Max(hoverTime + Time.deltaTime, 0.5f);
			if (hoverTime >= 0.75f) UIManager.inst.OnHoverSelect(this); //Display the Thing on Big Screen
		}
		else hoverTime = 0;
	}
	#endregion
}
