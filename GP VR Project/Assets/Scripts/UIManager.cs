using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using XellExtraUtils;

public class UIManager : MonoBehaviour
{
	[Header("General Components")]
	public static UIManager inst;
	public static bool isTransitioning; //Static Bool to prevent Button Events from Triggering as it Plays Animation
	[SerializeField] GameManager gm;
	[SerializeField] List<BeatmapData> bmds;
	[SerializeField] BeatmapUIObj songObjPrefab;

	[Header("UI Components")]
	[SerializeField] RectTransform content;
	[SerializeField] List<BeatmapUIObj> songObjs;
	[SerializeField] RectTransform songHolder;
	[SerializeField] List<BeatmapUIObj> beatmaps;
	[SerializeField] RectTransform beatmapHolder;
	[SerializeField] float sizePerNewObj; //How much Pxs does 1 new Entry take up. As of now 35

	[Header("For Animations")]
	[SerializeField] Animator anim;
	[SerializeField] int selectPhase = 0;

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
		beatmapHolder.sizeDelta = new Vector2(sizePerNewObj * bmd.mapInfos.Count, beatmapHolder.sizeDelta.y);
		//May want to use a different Scroll Rect just for Beatmap Select
		content.sizeDelta = new Vector2(songHolder.sizeDelta.x + sizePerNewObj * 6, content.sizeDelta.y);

		for (int i = 0; i < bmd.mapInfos.Count; i++)
		{
			BeatmapUIObj beatmapObj = ObjectPooling.inst.SpawnFromPool("Beatmap UI", beatmapHolder.transform.position, Quaternion.identity).GetComponent<BeatmapUIObj>();
			beatmapObj.AssignBmData(bmd);
			beatmapObj.AssignBmInfo(bmd.mapInfos[i]);
			beatmapObj.mainImg.SizeToFillParent();
			beatmaps.Add(beatmapObj);
		}
	}

	#region Animation Events
	public void BackToSongSelect()
	{
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

	public void SetTransitionStateFalse()
	{
		isTransitioning = false;
	}
	#endregion
}
