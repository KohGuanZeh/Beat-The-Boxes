using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using VRTK;

public class Glove : MonoBehaviour
{
	[Header("Glove Components")]
	public GameManager gm;
	public bool isLeft;
	public BoxColor gloveColor;
	[SerializeField] Renderer r; //To Change Material

	[SerializeField] VRTK_ControllerReference controllerRef;

	[SerializeField] bool uiPointerEnabled;
	[SerializeField] VRTK_UIPointer uiPointer;
	[SerializeField] VRTK_StraightPointerRenderer straightPointer;
	[SerializeField] VRTK_Pointer vrPointer;
	[SerializeField] VRTK_UIGraphicRaycaster raycaster;

	public delegate void VoidDelegate();
	VoidDelegate UpdateInitialiser;

	// Start is called before the first frame update
	void Start()
	{
		gm = GameManager.inst;

		controllerRef = VRTK_ControllerReference.GetControllerReference(gameObject);
		if (!VRTK_ControllerReference.IsValid(controllerRef)) UpdateInitialiser += InitiliaseController;

		SetGloveToDefaultColors();
		uiPointerEnabled = true;
	}

	// Update is called once per frame
	void Update()
	{
		//Run Initialisation Methods under Update. For Getting Controller Reference
		if (UpdateInitialiser != null) UpdateInitialiser();

		if (Input.GetKeyDown(KeyCode.X) && isLeft) ChangeGloveColor();
		if (Input.GetKeyDown(KeyCode.N) && !isLeft) ChangeGloveColor();

		if (gm.gameState == GameState.OnPlay)
		{
			if (uiPointerEnabled) EnableDisableUIPointer(false);
		}
		else if (!uiPointerEnabled) EnableDisableUIPointer(true);

		if (gm.gameState == GameState.Ended) SetGloveToDefaultColors();

		//Pause and Resume is in Game Manager Script and tied via Unity Events
		//print(string.Format("Controller Velocity: {0}. Direction is: {1}", GetControllerVelocity().sqrMagnitude, GetHitDirection(GetControllerVelocity().normalized)));
	}

	void InitiliaseController()
	{
		if (!VRTK_ControllerReference.IsValid(controllerRef)) controllerRef = VRTK_ControllerReference.GetControllerReference(gameObject);
		else UpdateInitialiser -= InitiliaseController;
	}

	public void SetGloveToDefaultColors()
	{
		if (isLeft)
		{
			gloveColor = BoxColor.Red;
			r.material = gm.gloveMats[0];
		}
		else
		{
			gloveColor = BoxColor.Blue;
			r.material = gm.gloveMats[1];
		}
	}

	public void ChangeGloveColor()
	{
		if (gm.gameState == GameState.OnPlay)
		{
			switch (gloveColor)
			{
				case BoxColor.Red:
					gloveColor = BoxColor.Yellow; //Alternate between Yellow and Red
					r.material = gm.gloveMats[3];
					break;
				case BoxColor.Blue:
					gloveColor = BoxColor.Green; //Alternate between Green and Blue
					r.material = gm.gloveMats[2];
					break;
				case BoxColor.Green:
					gloveColor = BoxColor.Blue; //Alternate between Green and Blue
					r.material = gm.gloveMats[1];
					break;
				case BoxColor.Yellow:
					gloveColor = BoxColor.Red; //Alternate between Yellow and Red
					r.material = gm.gloveMats[0];
					break;
			}
		}
	}

	public Vector3 GetControllerVelocity()
	{
		if (VRTK_ControllerReference.IsValid(controllerRef)) return VRTK_DeviceFinder.GetControllerVelocity(controllerRef);
		else return Vector3.zero;
	}

	void EnableDisableUIPointer(bool enable)
	{
		uiPointerEnabled = enable;
		uiPointer.enabled = enable;
		straightPointer.enabled = enable;
		vrPointer.enabled = enable;
	}

	/*public Direction GetHitDirection(Vector3 hitDir)
	{
		float x, y, z = 0;
		bool xPos, yPos, zPos = false;

		xPos = hitDir.x > 0;
		yPos = hitDir.y > 0;
		zPos = hitDir.z > 0;

		x = Mathf.Abs(hitDir.x);
		y = Mathf.Abs(hitDir.y);
		z = Mathf.Abs(hitDir.z);

		if (x > y && x > z) return xPos ? Direction.Right : Direction.Left;
		else if (y > x && y > z) return yPos ? Direction.Up : Direction.Down;
		else if (z > x && z > y) return zPos ? Direction.Forward : Direction.Backward;
		else return Direction.None;
	}*/
}
