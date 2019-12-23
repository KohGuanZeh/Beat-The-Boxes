using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using TMPro;

public enum UIObjType {Song, Beatmap};

public class BeatmapUIObj : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	[Header("General Components")]
	public UIObjType uiObjType;
	public Animator anim; //Should have Component by default
	public int bmdIndex; //From BMD List
	public int bmiIndex; //Need to Pass Index instead as Struct is Pass by Values. Not by Ref
	public ScrollRect scrollRect;
	public GraphicRaycaster raycaster;

	[Header ("For Displaying Splash Images")]
	//public Texture2D splashImg;
	public RawImage mainImg; //Should have Component by default
	public bool isHovering;
	public bool checkIfIsScrolling;
	public float checkTime;
	public float hoverTime;

	[Header("For Beatmap Select Only")]
	public TextMeshProUGUI difficulty;

	[Header("For Playing Correct Audio")]
	public AudioClip audio;

	void Update()
	{
		anim.SetFloat("Normalized Time", Mathf.PingPong(Time.time, 1));
		SelectOnHover();
	}

	public void AssignBmData(int bmdIndex, ScrollRect rect)
	{
		this.bmdIndex = bmdIndex;
		audio = OszUnpacker.bmds[bmdIndex].audio;
		mainImg.texture = OszUnpacker.bmds[bmdIndex].mainSplash;
		scrollRect = rect;
	}

	public void AssignBmInfo(int bmiIndex)
	{
		this.bmiIndex = bmiIndex;
		difficulty.text = OszUnpacker.bmds[bmdIndex].mapInfos[bmiIndex].difficulty.ToString("0.0");
	}

	#region Direct Button Functions
	//For Song Select UI Objs
	public void OnSongSelect()
	{
		if (UIManager.isTransitioning || scrollRect.velocity.sqrMagnitude > 5) return;

		print(scrollRect.velocity.sqrMagnitude);
		UIManager.isTransitioning = true; //Need Manually Set to True as it is not Linked with Canvas UI
		UIManager.inst.SaveScrollPosition();
		anim.SetBool("Clicked", true);
		anim.SetBool("Is Hovering", false);

		UIManager.inst.PopulateBeatmapSelect(bmdIndex);
		UIManager.inst.SuscribeToOnTransitionEvents(false, ResetButtonAnimator);
		UIManager.inst.OnHoverSelect(this); //Display the Thing on Big Screen
	}

	//For Beatmap Select UI Objs
	public void OnBeatmapSelect()
	{
		if (UIManager.isTransitioning || scrollRect.velocity.sqrMagnitude > 5) return;

		print(scrollRect.velocity.sqrMagnitude);
		UIManager.isTransitioning = true; //Need Manually Set to True as it is not Linked with Canvas UI
		UIManager.inst.SaveScrollPosition();
		anim.SetBool("Clicked", true);
		anim.SetBool("Is Hovering", false);

		GameManager.inst.AssignMapData(bmdIndex, bmiIndex);
		UIManager.inst.SuscribeToOnTransitionEvents(false, () => UIManager.inst.SuscribeToOnTransitionEvents(true, () => UIManager.inst.SuscribeToOnTransitionEvents(false, ResetButtonAnimator)));
		UIManager.inst.OnHoverSelect(this); //Display the Thing on Big Screen
	}
	#endregion

	#region Animation Events
	void OnFade()
	{
		switch (uiObjType)
		{
			case UIObjType.Beatmap:
				UIManager.inst.OnBeatmapSelect();
				break;
			case UIObjType.Song:
				UIManager.inst.OnSongSelect();
				break;
		}
	}

	void ResetButtonAnimator()
	{
		anim.SetBool("Is Hovering", false);
		anim.SetBool("Clicked", false);
		anim.Update(10);
		//UIManager.inst.OnTransitionEnd -= ResetButtonAnimator; //Called Through Delegate so that it can be handled on UIManager End instead
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
