using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using VRTK;

public class Glove : MonoBehaviour
{
	[Header ("Glove Components")]
	public GameManager gm;
	public bool isLeft;
	public BoxColor gloveColor;
	[SerializeField] Renderer r; //To Change Material

	VRTK_ControllerReference controllerRef;
	VRTK_ControllerHaptics controllerHaptics;


    // Start is called before the first frame update
    void Start()
    {
		gm = GameManager.inst;

		if (isLeft)
		{
			gloveColor = BoxColor.Red;
			r.material = gm.mats[0];
		}
		else
		{
			gloveColor = BoxColor.Blue;
			r.material = gm.mats[1];
		}
	}

    // Update is called once per frame
    void Update()
    {
        
    }

	public void ChangeGloveColor()
	{
		switch (gloveColor)
		{
			case BoxColor.Red:
				gloveColor = BoxColor.Yellow; //Alternate between Yellow and Red
				r.material = gm.mats[3];
				break;
			case BoxColor.Blue:
				gloveColor = BoxColor.Green; //Alternate between Green and Blue
				r.material = gm.mats[2];
				break;
			case BoxColor.Green:
				gloveColor = BoxColor.Blue; //Alternate between Green and Blue
				r.material = gm.mats[1];
				break;
			case BoxColor.Yellow:
				gloveColor = BoxColor.Red; //Alternate between Yellow and Red
				r.material = gm.mats[0];
				break;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		
	}
}
