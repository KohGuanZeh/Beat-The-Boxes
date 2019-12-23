using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Audio;
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

	[System.Serializable]
	public struct UIPanelComponents
	{
		public string name; //Name for Readability in Inspector
		public GameObject panel;
		public GameObject buttonGroup; //Some will Share the Same Button Group. Ie Song Select + Beatmap Select
	}

	[Header("General Components")]
	public static UIManager inst;
	public static bool isTransitioning; //Static Bool to prevent Button Events from Triggering as it Plays Animation
	[SerializeField] GameManager gm;
	[SerializeField] BeatmapUIObj songObjPrefab;
	[SerializeField] GraphicRaycaster raycaster;

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
	[SerializeField] UIPanelComponents[] allPanelGroups; //0 is Song Select, 1 is Beatmap Select, 2 is Options, 3 is Instructions, 4 is Credits, 5 is Pause Screen, 6 is Game End, 7 is Main Menu

	[Header("UI Panel Song Select")]
	[SerializeField] TextMeshProUGUI songTitle;
	[SerializeField] TextMeshProUGUI artist;
	[SerializeField] TextMeshProUGUI mapCount;

	[Header("UI Panel Beatmap Select")]
	[SerializeField] TextMeshProUGUI mapTitle;
	[SerializeField] TextMeshProUGUI creatorName;
	[SerializeField] Image difficulty;
	[SerializeField] ScoreUIComponents highestScore;
	[SerializeField] ScoreUIComponents[] highScores;

	[Header("UI Panel Pause")]
	[SerializeField] TextMeshProUGUI optionsTitle;
	[SerializeField] AudioMixer mixer;
	[SerializeField] Slider[] sliders;

	[Header("GUI")]
	[SerializeField] GameObject scoreItems;
	[SerializeField] TextMeshProUGUI currentScore, scoreFollowUp;
	[SerializeField] TextMeshProUGUI combo, comboFollowUp;
	[SerializeField] TextMeshProUGUI scoreMult, scoreMultFollowUp;
	public GameObject triggerClickToContinue;

	[Header("End Screen")]
	[SerializeField] GameObject newHighScore;
	[SerializeField] ScoreUIComponents endScore;

	[Header("For Clearing HighScores")]
	[SerializeField] TextMeshProUGUI mapTitlePrompt;

	[Header("For Scroll Rect")]
	[SerializeField] ScrollRect scroll;
	[SerializeField] Vector2 songSelectNormalizedPos;

	[Header("For Button SFX")]
	[SerializeField] AudioSource buttonSfxPlayer;
	[SerializeField] AudioClip[] buttonSounds;

	[Header("For Animations")]
	[SerializeField] Animator anim;
	[SerializeField] int selectPhase = 0;

	public delegate void UITransitionDel();
	UITransitionDel OnTransitionEnd;
	UITransitionDel OnTransitionStart;

	private void Awake()
	{
		inst = this;
	}

	// Start is called before the first frame update
	void Start()
	{
		gm = GameManager.inst;
		anim = GetComponent<Animator>();
		songObjs = new List<BeatmapUIObj>();
		beatmaps = new List<BeatmapUIObj>();

		print(songObjs.Count);
		PopulateSongSelect();

		OnTransitionEnd += () =>
		{
			OnTransitionStart += () =>
			{
				allPanelGroups[7].buttonGroup.SetActive(true);
			};
		};

		sliders[0].value = PlayerPrefs.GetFloat("Master Volume", 1);
		sliders[1].value = PlayerPrefs.GetFloat("Music Volume", 1);
		sliders[2].value = PlayerPrefs.GetFloat("Sound Volume", 1);

		buttonSfxPlayer.ignoreListenerPause = true;
	}

	public void UpdateScores(int score, int combo, int scoreMult)
	{
		currentScore.text = score.ToString("000000000");
		scoreFollowUp.text = currentScore.text;
		this.combo.text = string.Format("x{0}", combo);
		comboFollowUp.text = this.combo.text;
		this.scoreMult.text = string.Format("x{0}", scoreMult);
		scoreMultFollowUp.text = this.scoreMult.text;

		anim.SetTrigger("On Score");
	}

	public void PopulateSongSelect()
	{
		foreach (BeatmapUIObj songObj in songObjs) Destroy(songObj);

		songHolder.sizeDelta = new Vector2(sizePerNewObj * OszUnpacker.bmds.Count, songHolder.sizeDelta.y);
		content.sizeDelta = new Vector2(songHolder.sizeDelta.x + sizePerNewObj * 6, content.sizeDelta.y); //6 because can view max 7 at once. 3 at start, 3 at end. Required so that the First and Last Obj can be at the middle

		for (int i = 0; i < OszUnpacker.bmds.Count; i++)
		{
			OszUnpacker.bmds = OszUnpacker.LoadBeatmapData();
			BeatmapUIObj songObj = Instantiate(songObjPrefab, songHolder);
			songObj.transform.position = new Vector3(songObj.transform.position.x, songObj.transform.position.y, songObj.transform.position.z - 0.01f);
			songObj.AssignBmData(i, scroll);
			songObj.mainImg.SizeToFillParent();
			songObjs.Add(songObj);
		}
	}

	public void PopulateBeatmapSelect(int bmdIndex)
	{
		BeatmapData bmd = OszUnpacker.bmds[bmdIndex];
		foreach (BeatmapUIObj beatmap in beatmaps) ObjectPooling.inst.ReturnToPool(beatmap.gameObject, "Beatmap UI");
		beatmaps.Clear();

		beatmapHolder.sizeDelta = new Vector2(sizePerNewObj * bmd.mapInfos.Count, beatmapHolder.sizeDelta.y);
		//May want to use a different Scroll Rect just for Beatmap Select


		for (int i = 0; i < bmd.mapInfos.Count; i++)
		{
			BeatmapUIObj beatmapObj = ObjectPooling.inst.SpawnFromPool("Beatmap UI", beatmapHolder.transform.position, beatmapHolder.transform.rotation).GetComponent<BeatmapUIObj>();
			beatmapObj.AssignBmData(bmdIndex, scroll);
			beatmapObj.AssignBmInfo(i);
			beatmapObj.mainImg.SizeToFillParent();
			beatmaps.Add(beatmapObj);
		}
	}

	#region For Beatmap and Song Select
	public void SelectRandomSongOnStart()
	{
		int index = Random.Range(0, songObjs.Count);
		content.anchoredPosition = new Vector2(-35 * index, content.anchoredPosition.y);
		scroll.normalizedPosition = scroll.normalizedPosition;
		//songObjs[index].anim.SetBool("Is Hovering", true);
		OnHoverSelect(songObjs[index]);
	}

	public void SelectSpecificSong(int index)
	{
		if (index >= songObjs.Count) return;
		content.anchoredPosition = new Vector2(-35 * index, content.anchoredPosition.y);
		scroll.normalizedPosition = scroll.normalizedPosition;
		//songObjs[index].anim.SetBool("Is Hovering", true);
		OnHoverSelect(songObjs[index]);
	}

	public bool CanTriggerScrollButtons()
	{
		//print(scroll.velocity.sqrMagnitude);
		return scroll.velocity.sqrMagnitude < 1f;
	}

	public void OnHoverSelect(BeatmapUIObj uiObj, bool forceInvoke = false)
	{
		switch (uiObj.uiObjType)
		{
			case UIObjType.Song:

				if (currentSelectedSong == uiObj && !forceInvoke) return;

				currentSelectedSong = uiObj;

				songTitle.text = OszUnpacker.bmds[currentSelectedSong.bmdIndex].songName;
				artist.text = OszUnpacker.bmds[currentSelectedSong.bmdIndex].artistName;
				mapCount.text = string.Format("{0} Beatmaps", OszUnpacker.bmds[currentSelectedSong.bmdIndex].mapInfos.Count);

				splashImg.color = Color.white;
				splashImg.texture = OszUnpacker.bmds[currentSelectedSong.bmdIndex].mainSplash;
				splashImg.SizeToFillParent();

				gm.songPlayer.clip = currentSelectedSong.audio;
				gm.songPlayer.Play();

				PlayButtonSound(0);

				anim.SetTrigger("New Select");
				break;

			case UIObjType.Beatmap:

				if (currentSelectedBeatmap == uiObj && !forceInvoke) return;

				currentSelectedBeatmap = uiObj;

				BeatmapInfo bmi = OszUnpacker.bmds[currentSelectedSong.bmdIndex].mapInfos[currentSelectedBeatmap.bmiIndex];

				mapTitle.text = bmi.mapName;
				creatorName.text = string.Format("Mapped by: {0}", bmi.creator);
				difficulty.fillAmount = bmi.difficulty / 10;

				mapTitlePrompt.text = bmi.mapName;

				RepopulateHighscores(bmi);

				PlayButtonSound(0);

				anim.SetTrigger("New Select");
				break;
		}
	}
	#endregion

	#region For Score Related Population
	public void RepopulateHighscores(BeatmapInfo bmi)
	{
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
				highestScore.parentObject.SetActive(true);
				highestScore.grade.sprite = GetGradeSprite(bmi.scores[i].grade);
				highestScore.score.text = bmi.scores[i].score.ToString("000000000");
				highestScore.combo.text = string.Format("x{0}", bmi.scores[i].maxCombo);
				highestScore.hits.text = string.Format("Hits: {0}", bmi.scores[i].hits);
				highestScore.miss.text = string.Format("Miss: {0}", bmi.scores[i].miss);
			}

			highScores[i].parentObject.SetActive(true);
			highScores[i].grade.sprite = GetGradeSprite(bmi.scores[i].grade);
			highScores[i].score.text = bmi.scores[i].score.ToString("000000000");
			highScores[i].combo.text = string.Format("x{0}", bmi.scores[i].maxCombo);
			highScores[i].hits.text = string.Format("Hits: {0}", bmi.scores[i].hits);
			highScores[i].miss.text = string.Format("Miss: {0}", bmi.scores[i].miss);
		}
	}
	
	public void PopulateScore(ScoreInfo scoreInfo, bool isNewHighscore)
	{
		newHighScore.SetActive(isNewHighscore);

		endScore.grade.sprite = GetGradeSprite(scoreInfo.grade);
		endScore.score.text = scoreInfo.score.ToString("000000000");
		endScore.combo.text = string.Format("x{0}", scoreInfo.maxCombo);
		endScore.hits.text = string.Format("Hits: {0}", scoreInfo.hits);
		endScore.miss.text = string.Format("Miss: {0}", scoreInfo.miss);
	}
	#endregion

	#region Direct Button Functions
	public void PlayButtonSound(int clip)
	{
		buttonSfxPlayer.clip = buttonSounds[clip];
		buttonSfxPlayer.Play();
	}

	public void StartGame()
	{
		if (isTransitioning) return; //Prevent Multiple Calls

		//Transit to Song Select
		isTransitioning = true;
		anim.SetTrigger("Trigger Clicked");
		SuscribeToOnTransitionEvents(false, () => anim.ResetTrigger("Trigger Clicked"));
	}

	public void BackToSongSelect()
	{
		if (selectPhase == 0 || isTransitioning) return;

		isTransitioning = true;
		selectPhase = 0;
		anim.SetTrigger("Trigger Clicked");
		anim.SetInteger("Select Phase", selectPhase);

		SuscribeToOnTransitionEvents(false, () => anim.ResetTrigger("Trigger Clicked"));
	}

	public void ContinueGame()
	{
		if (isTransitioning) return;

		isTransitioning = true;
		anim.SetBool("Close Menu", true);
		SuscribeToOnTransitionEvents(false, CloseAllPanels);
	}

	public void Retry()
	{
		if (isTransitioning) return;

		isTransitioning = true;

		switch (gm.gameState)
		{
			case GameState.Paused:
				anim.SetBool("Close Menu", true);
				gm.Retry();
				break;
			case GameState.Ended:
				OnBeatmapSelect();
				break;
		}
	}

	public void FromGameToMain()
	{
		if (isTransitioning) return;

		switch (gm.gameState)
		{
			case GameState.Paused:
				isTransitioning = true;

				gm.songPlayer.Stop();
				gm.ReturnAllSpawnedBeatsToPool();
				AudioListener.pause = false;

				gm.songPlayer.loop = true;

				gm.gameState = GameState.Menu;
				selectPhase = 1;
				anim.SetInteger("Select Phase", selectPhase);

				anim.SetBool("Hide Score", true);
				anim.SetTrigger("Trigger Hide");
				anim.SetBool("Rotate To Side", false);
				anim.SetTrigger("Rotate");

				SuscribeToOnTransitionEvents(false, () =>
				{
					anim.SetBool("Close Menu", true);
					SuscribeToOnTransitionEvents(true, () => SuscribeToOnTransitionEvents(false, () => ReopenPanel(1, true)));
					SuscribeToOnTransitionEvents(true, () => SuscribeToOnTransitionEvents(false, () => OnHoverSelect(currentSelectedSong, true)));
				});
				break;
			case GameState.Ended:
				gm.songPlayer.loop = true;

				gm.gameState = GameState.Menu;
				selectPhase = 1;
				anim.SetInteger("Select Phase", selectPhase);

				CloseOpenMenu(1);
				break;
		}
	}

	public void QuitGame()
	{
		if (isTransitioning) return;

		print("Quit");
		Application.Quit();
	}

	public void OnOptionsScreenOpen(bool inGame = false)
	{
		optionsTitle.text = inGame ? "Paused" : "Options";
	}

	public void CloseOpenMenu(int panelIndex)
	{
		if (isTransitioning) return;

		isTransitioning = true;
		anim.SetBool("Close Menu", true);

		bool showMainMenu = false;

		if (panelIndex == 0 || panelIndex == 1)
		{
			if (gm.gameState == GameState.Menu)
			{
				panelIndex = selectPhase; //For Closing and Opening back to Song/Beatmap Select
				showMainMenu = true;
			}
			else panelIndex = 5; //Show Pause Screen Instead if Game State is not Menu
		}

		OnTransitionEnd += () => ReopenPanel(panelIndex, showMainMenu);
	}

	public void ShowEndScore()
	{
		anim.SetBool("Hide Score", true);
		anim.SetTrigger("Trigger Hide");
		ReopenPanel(6);

		SuscribeToOnTransitionEvents(false, () =>
		{
			anim.SetBool("Rotate To Side", false);
			anim.SetTrigger("Rotate");
		});
	}

	public void ClearHighScores(bool clear)
	{
		if (clear)
		{
			BeatmapInfo bmi = OszUnpacker.bmds[currentSelectedSong.bmdIndex].mapInfos[currentSelectedBeatmap.bmiIndex];
			bmi.scores = new List<ScoreInfo>();
			OszUnpacker.bmds[currentSelectedSong.bmdIndex].mapInfos[currentSelectedBeatmap.bmiIndex] = bmi;
			OszUnpacker.SaveBeatmapData(OszUnpacker.bmds);
			RepopulateHighscores(bmi);
		}

		CloseOpenMenu(1);
	}
	#endregion

	#region Slider Functions
	public void SetMasterVolume(float val)
	{
		mixer.SetFloat("Master Volume", Mathf.Log10(val) * 20);
		PlayerPrefs.SetFloat("Master Volume", val);
	}

	public void SetMusicVolume(float val)
	{
		mixer.SetFloat("Music Volume", Mathf.Log10(val) * 20);
		PlayerPrefs.SetFloat("Music Volume", val);
	}

	public void SetSoundVolume(float val)
	{
		mixer.SetFloat("Sound Volume", Mathf.Log10(val) * 20);
		PlayerPrefs.SetFloat("Sound Volume", val);
	}
	#endregion

	#region Functions Called in Beatmap UI Objects
	public void OnSongSelect()
	{
		selectPhase = 1; //Set to Beatmap Select
		anim.SetTrigger("Trigger Clicked");
		anim.SetInteger("Select Phase", selectPhase);

		SuscribeToOnTransitionEvents(false, () => anim.ResetTrigger("Trigger Clicked"));
	}

	public void OnBeatmapSelect()
	{
		selectPhase = 2;
		anim.SetInteger("Select Phase", selectPhase);

		anim.SetBool("Rotate To Side", true);
		anim.SetTrigger("Rotate");

		SuscribeToOnTransitionEvents(false, () => ClosePanel(7));

		SuscribeToOnTransitionEvents(false, () =>
		{
			anim.SetBool("Hide Score", false);
			anim.SetTrigger("Trigger Hide");
		});

		SuscribeToOnTransitionEvents(false, () => {
			anim.SetBool("Close Menu", true);
			SuscribeToOnTransitionEvents(true, () => SuscribeToOnTransitionEvents(false, CloseAllPanels));
			SuscribeToOnTransitionEvents(true, () => SuscribeToOnTransitionEvents(false, gm.InitialiseBeatMap));
		});
	}

	public void SaveScrollPosition()
	{
		songSelectNormalizedPos = scroll.normalizedPosition;
	}
	#endregion

	#region Animation Events
	public void OpenPanel(int panelIndex)
	{
		allPanelGroups[panelIndex].panel.SetActive(true);
		allPanelGroups[panelIndex].buttonGroup.SetActive(true);
	}

	public void ClosePanel(int panelIndex)
	{
		allPanelGroups[panelIndex].panel.SetActive(false);
		allPanelGroups[panelIndex].buttonGroup.SetActive(false);
	}

	public void CloseAllPanels()
	{
		foreach (UIPanelComponents panelGroup in allPanelGroups)
		{
			panelGroup.panel.SetActive(false);
			panelGroup.buttonGroup.SetActive(false);
		}
	}

	public void ReopenPanel(int panelIndex, bool showMainMenu = false)
	{
		foreach (UIPanelComponents panelGroup in allPanelGroups)
		{
			panelGroup.panel.SetActive(false);
			panelGroup.buttonGroup.SetActive(false);
		}

		allPanelGroups[panelIndex].panel.SetActive(true);
		allPanelGroups[panelIndex].buttonGroup.SetActive(true);

		if (showMainMenu)
		{
			allPanelGroups[7].panel.SetActive(true);
			allPanelGroups[7].buttonGroup.SetActive(true);
		}

		anim.SetBool("Close Menu", false);
	}

	public void SongBeatmapSwapInterim()
	{
		if (selectPhase == 1)
		{
			allPanelGroups[0].panel.SetActive(false);
			allPanelGroups[1].panel.SetActive(true);

			//Reset Scroll Rect
			content.sizeDelta = new Vector2(beatmapHolder.sizeDelta.x + sizePerNewObj * 6, content.sizeDelta.y);
			scroll.normalizedPosition = Vector2.zero;
			OnHoverSelect(beatmaps[0]);
		}
		else
		{
			allPanelGroups[0].panel.SetActive(true);
			allPanelGroups[1].panel.SetActive(false);

			//For Resetting Scroll Rect
			content.sizeDelta = new Vector2(songHolder.sizeDelta.x + sizePerNewObj * 6, content.sizeDelta.y);
			scroll.normalizedPosition = songSelectNormalizedPos; //Value of its Previous Position is Recorded when Button is Pressed
		}
	}

	public void CallTransitionEvent(int isStart)
	{
		if (isStart == 1)
		{
			if (anim.GetCurrentAnimatorStateInfo(0).speed == 1) CallOnTransitionStart();
			else CallOnTransitionEnd(); //If Animation Event is Marked as Start but it is at -1 Speed, It should Call the End Transition Functions
		}
		else
		{
			if (anim.GetCurrentAnimatorStateInfo(0).speed == 1) CallOnTransitionEnd();
			else CallOnTransitionStart(); //If Animation Event is Marked as End but it is at -1 Speed, It should Call the Start Transition Functions
		}

		//print("Is Transitioning = " + isTransitioning + ", Anim Speed = " + anim.GetCurrentAnimatorStateInfo(0).speed);
	}

	public void SuscribeToOnTransitionEvents(bool isStart, UITransitionDel function)
	{
		if (isStart) OnTransitionStart += function;
		else OnTransitionEnd += function;
	}

	void CallOnTransitionStart()
	{
		//print("Transition Start Called on " + Time.time);
		if (OnTransitionStart != null) OnTransitionStart();
		OnTransitionStart = null; //Clear all Functions Suscribed to Delegate
		anim.StopPlayback();
		isTransitioning = true;
	}

	void CallOnTransitionEnd()
	{
		if (OnTransitionEnd != null) OnTransitionEnd();
		OnTransitionEnd = null; //Clear all Functions Suscribed to Delegate
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
