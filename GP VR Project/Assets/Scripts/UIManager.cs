using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using TMPro;

using XellExtraUtils;

public class UIManager : MonoBehaviour
{
	[System.Serializable]
	public struct ScoreUIComponents
	{
		public GameObject parentObject;
		public Image grade;
		public TextMeshProUGUI score;
		public TextMeshProUGUI combo;
		public TextMeshProUGUI hits;
		public TextMeshProUGUI miss;
	}

	[Header("General Components")]
	public static UIManager inst;
	public static bool isTransitioning; //Static Bool to prevent Button Events from Triggering as it Plays Animation
	[SerializeField] GameManager gm;
	[SerializeField] List<BeatmapData> bmds;
	[SerializeField] BeatmapUIObj songObjPrefab;

	[Header("For Beatmap and Song Select")]
	[SerializeField] RectTransform content;
	[SerializeField] BeatmapUIObj currentSelectedSong;
	[SerializeField] List<BeatmapUIObj> songObjs;
	[SerializeField] RectTransform songHolder;
	[SerializeField] BeatmapUIObj currentSelectedBeatmap;
	[SerializeField] List<BeatmapUIObj> beatmaps;
	[SerializeField] RectTransform beatmapHolder;
	[SerializeField] float sizePerNewObj; //How much Pxs does 1 new Entry take up. As of now 35

	[Header("UI Panel General")]
	[SerializeField] RawImage splashImg;
	[SerializeField] Sprite[] grades;

	[Header("UI Panel Song Select")]
	[SerializeField] GameObject songSelectGroup;
	[SerializeField] TextMeshProUGUI songTitle;
	[SerializeField] TextMeshProUGUI artist;
	[SerializeField] TextMeshProUGUI mapCount;

	[Header("UI Panel Beatmap Select")]
	[SerializeField] GameObject beatmapSelectGroup;
	[SerializeField] TextMeshProUGUI mapTitle;
	[SerializeField] TextMeshProUGUI creatorName;
	[SerializeField] Image difficulty;
	[SerializeField] ScoreUIComponents highestScore;
	[SerializeField] ScoreUIComponents[] highScores;

	[Header("For Scroll Rect")]
	[SerializeField] ScrollRect scroll;
	[SerializeField] Vector2 songSelectNormalizedPos;

	[Header("For Animations")]
	[SerializeField] Animator anim;
	[SerializeField] int selectPhase = 0;
	public delegate void UIDelegate();
	public UIDelegate OnTransitionEnd;

	private void Awake()
	{
		inst = this;
	}

	// Start is called before the first frame update
	void Start()
    {
		gm = GameManager.inst;
		bmds = OszUnpacker.bmds;
		anim = GetComponent<Animator>();
		songObjs = new List<BeatmapUIObj>();
		beatmaps = new List<BeatmapUIObj>();

		print(songObjs.Count);
		PopulateSongSelect();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void PopulateSongSelect()
	{
		songHolder.sizeDelta = new Vector2(sizePerNewObj * bmds.Count, songHolder.sizeDelta.y);
		content.sizeDelta = new Vector2(songHolder.sizeDelta.x + sizePerNewObj * 6, content.sizeDelta.y); //6 because can view max 7 at once. 3 at start, 3 at end. Required so that the First and Last Obj can be at the middle

		for (int i = 0; i < bmds.Count; i++)
		{
			OszUnpacker.bmds = OszUnpacker.LoadBeatmapData();
			BeatmapUIObj songObj = Instantiate(songObjPrefab, songHolder);
			songObj.transform.position = new Vector3(songObj.transform.position.x, songObj.transform.position.y, songObj.transform.position.z - 0.01f);
			songObj.AssignBmData(bmds[i]);
			songObj.mainImg.SizeToFillParent();
			songObjs.Add(songObj);
		}
	}

	public void PopulateBeatmapSelect(BeatmapData bmd)
	{
		foreach (BeatmapUIObj beatmap in beatmaps) ObjectPooling.inst.ReturnToPool(beatmap.gameObject, "Beatmap UI");
		beatmaps.Clear();

		beatmapHolder.sizeDelta = new Vector2(sizePerNewObj * bmd.mapInfos.Count, beatmapHolder.sizeDelta.y);
		//May want to use a different Scroll Rect just for Beatmap Select
		

		for (int i = 0; i < bmd.mapInfos.Count; i++)
		{
			BeatmapUIObj beatmapObj = ObjectPooling.inst.SpawnFromPool("Beatmap UI", beatmapHolder.transform.position, beatmapHolder.transform.rotation).GetComponent<BeatmapUIObj>();
			beatmapObj.AssignBmData(bmd);
			beatmapObj.AssignBmInfo(bmd.mapInfos[i]);
			beatmapObj.mainImg.SizeToFillParent();
			beatmaps.Add(beatmapObj);
		}
	}

	#region For Beatmap and Song Select
	public bool CanTriggerScrollButtons()
	{
		print(scroll.velocity.sqrMagnitude);
		return scroll.velocity.sqrMagnitude < 1f;
	}

	public void OnHoverSelect(BeatmapUIObj uiObj)
	{
		switch (uiObj.uiObjType)
		{
			case UIObjType.Song:

				if (currentSelectedSong == uiObj) return;

				currentSelectedSong = uiObj;

				songTitle.text = currentSelectedSong.bmd.songName;
				artist.text = currentSelectedSong.bmd.artistName;
				mapCount.text = string.Format("{0} Beatmaps", currentSelectedSong.bmd.mapInfos.Count);

				splashImg.color = Color.white;
				splashImg.texture = currentSelectedSong.bmd.mainSplash;
				splashImg.SizeToFillParent();

				gm.songPlayer.clip = currentSelectedSong.audio;
				gm.songPlayer.Play();

				anim.SetTrigger("New Select");
				break;

			case UIObjType.Beatmap:

				if (currentSelectedBeatmap == uiObj) return;

				currentSelectedBeatmap = uiObj;

				BeatmapInfo bmi = currentSelectedBeatmap.bmi;

				mapTitle.text = bmi.mapName;
				creatorName.text = string.Format("Mapped by: {0}", bmi.creator);
				difficulty.fillAmount = bmi.difficulty / 10;

				for (int i = 0; i < 3; i++)
				{
					if ((bmi.scores.Count - 1) < i)
					{
						if (i == 0) highestScore.parentObject.SetActive(false);
						highScores[i].parentObject.SetActive(false);
						continue;
					} 

					if (i == 0)
					{
						highestScore.grade.sprite = GetGradeSprite(bmi.scores[i].grade);
						highestScore.score.text = bmi.scores[i].score.ToString("000000000");
						highestScore.combo.text = string.Format("x{0}", bmi.scores[i].maxCombo);
						highestScore.hits.text = string.Format("Hits: {0}", bmi.scores[i].hits);
						highestScore.miss.text = string.Format("Miss: {0}", bmi.scores[i].miss);
					}

					highScores[i].grade.sprite = GetGradeSprite(bmi.scores[i].grade);
					highScores[i].score.text = bmi.scores[i].score.ToString("000000000");
					highScores[i].combo.text = string.Format("x{0}", bmi.scores[i].maxCombo);
					highScores[i].hits.text = string.Format("Hits: {0}", bmi.scores[i].hits);
					highScores[i].miss.text = string.Format("Miss: {0}", bmi.scores[i].miss);
				}

				anim.SetTrigger("New Select");
				break;
		}
	}
	#endregion

	#region Button and Animation Events
	public void BackToSongSelect()
	{
		if (selectPhase == 0 || isTransitioning) return;

		selectPhase = 0;
		anim.SetInteger("Select Phase", selectPhase);
	}

	public void OnSongSelect()
	{
		selectPhase = 1; //Set to Beatmap Select
		anim.SetInteger("Select Phase", selectPhase);
	}

	public void OnBeatmapSelect()
	{
		selectPhase = 2;
		anim.SetInteger("Select Phase", selectPhase);
	}

	public void OnMenuClosed()
	{
		gm.InitialiseBeatMap();
	}

	public void SongBeatmapSwapInterim()
	{
		if (selectPhase == 1)
		{
			//For Resetting Scroll Rect
			songSelectNormalizedPos = scroll.normalizedPosition;
			content.sizeDelta = new Vector2(beatmapHolder.sizeDelta.x + sizePerNewObj * 6, content.sizeDelta.y);

			scroll.normalizedPosition = Vector2.zero;

			OnHoverSelect(beatmaps[0]);
		}
		else
		{
			//For Resetting Scroll Rect
			content.sizeDelta = new Vector2(songHolder.sizeDelta.x + sizePerNewObj * 6, content.sizeDelta.y);
			scroll.Rebuild(CanvasUpdate.MaxUpdateValue);

			scroll.normalizedPosition = songSelectNormalizedPos;
		} 
	}

	public void CallOnTransitionEnd()
	{
		if (OnTransitionEnd != null) OnTransitionEnd();
	}

	public void SetTransitionStateFalse()
	{
		isTransitioning = false;
	}
	#endregion

	#region Other Modular Functions
	Sprite GetGradeSprite(Grade grade)
	{
		switch (grade)
		{
			case Grade.S:
				return grades[0];
			case Grade.A:
				return grades[1];
			case Grade.B:
				return grades[2];
			case Grade.C:
				return grades[3];
			case Grade.D:
				return grades[4];
			default:
				return grades[4];
		}
	}
	#endregion
}
