using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XellExtraUtils;

public enum BoxColor {None, Red, Blue, Green, Yellow }

public enum BoxType {Normal, Iron, Large } //Normal is Small Beats, Big requires Continuous Punch, Iron requires Dodging

public class Beat : MonoBehaviour
{
	//0 is Red, 1 is Blue, 2 is Green, 3 is Yellow, 4 is Iron, 5 is Large

	[Header("General Components")]
	[SerializeField] GameManager gm;
	[SerializeField] float startXDisplaceVal = 0.5f;
	[SerializeField] float finalXDisplaceVal = 0.15f;
	[SerializeField] Transform beatMeshObj;

	[Header("Beat Attributes")]
	public BoxType boxType;
	public BoxColor boxColor;

	[Header("Beat Rotation")]
	[SerializeField] float minRot;
	[SerializeField] float maxRot; //Should always be positive
	[SerializeField] Vector3 rotation;

	[Header("Set Target Time and Position")]
	[SerializeField] double spawnedTime;
	[SerializeField] double targetTime;
	[SerializeField] double destroyTime;
	[SerializeField] Vector3 spawnPos;
	[SerializeField] float spawnHitDist; //Z Displacement
	[SerializeField] float xDisplacement; //X Displacement
	[SerializeField] float finalXDisplacement; //Final X Displacement upon reaching Hit Pos
	[SerializeField] float yMultiplier = 1; 
	[SerializeField] AnimationCurve yPosOverTime; //Y Displacement

	// Update is called once per frame
	void Update()
    {
		MoveBeatPosition();
		Rotate();

		if (gm.autoMode) AutoHitBeat();
    }

	#region For Beat Initialisation
	public void AssignGM(GameManager gm)
	{
		if (this.gm == null) this.gm = gm;
	}

	public void InitialiseBeat(double spawnedTime, double targetTime, double destroyTime, Vector3 spawnPos, float spawnHitDist)
	{
		beatMeshObj = transform.GetChild(0);

		this.spawnedTime = spawnedTime;
		this.targetTime = targetTime / 1000;
		this.destroyTime = destroyTime / 1000;

		this.spawnPos = spawnPos;
		this.spawnHitDist = spawnHitDist;

		if (boxType == BoxType.Normal)
		{
			switch (boxColor)
			{
				case BoxColor.Red:
					SetDisplacement(false);
					break;
				case BoxColor.Yellow:
					SetDisplacement(false);
					break;
				case BoxColor.Blue:
					SetDisplacement(true);
					break;
				case BoxColor.Green:
					SetDisplacement(true);
					break;
			}
		}
		else if (boxType == BoxType.Iron) SetDisplacement(Random.Range(0f, 1f) >= 0.5f ? true : false);

		yMultiplier = transform.localScale.x > 1 ? 1 : 0;

		//X Rotation should always be Negative. Y and Z rotation (+/-) will be random
		rotation = new Vector3(-Random.Range(minRot, maxRot),
								MathFunctions.RandomisePositiveNegative() * Random.Range(minRot, maxRot),
								MathFunctions.RandomisePositiveNegative() * Random.Range(minRot, maxRot));
	}

	void SetDisplacement(bool isPositive)
	{
		if (isPositive)
		{
			xDisplacement = startXDisplaceVal;
			finalXDisplacement = finalXDisplaceVal;
		}
		else
		{
			xDisplacement = -startXDisplaceVal;
			finalXDisplacement = -finalXDisplaceVal;
		}
	}
	#endregion

	#region For Beat Hit
	public void AutoHitBeat()
	{
		if (GetOffsetRatio() >= 1.0f)
		{
			switch (boxType)
			{
				case BoxType.Normal:
					if (GetOffsetRatio() >= 1.0f) Hit();
					break;
				case BoxType.Large:
					if (GetOffsetRatio() >= 1.0f) Hit(false);
					break;
			}
		}
	}

	public void Hit(bool destroy = true)
	{
		//Play Sound Effect
		gm.AddScore();
		if (destroy) Destroy(gameObject);
	}
	#endregion

	#region For Beat Movement
	public void MoveBeatPosition()
	{
		float zOffset = Mathf.LerpUnclamped(0, spawnHitDist, GetOffsetRatio());
		float xOffset = xDisplacement; //Mathf.Lerp(xDisplacement, finalXDisplacement, GetOffsetRatio());
		float yOffset = yMultiplier; //boxType == BoxType.Large ? 0 : yPosOverTime.Evaluate(GetOffsetRatio()) * yMultiplier;
		transform.position = spawnPos + new Vector3(xOffset, yOffset, zOffset);
		
		//if (GetOffsetRatio() >= 1.0f) Destroy(gameObject);
	}

	float GetOffsetRatio()
	{
		return (float)((gm.GetTrackTime() - spawnedTime) / (targetTime - spawnedTime));
	}
	#endregion

	#region For Beat Rotation
	void Rotate()
	{
		transform.Rotate(rotation * Time.deltaTime);
	}
	#endregion

	#region Trigger Functions
	private void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Destroy Zone")
		{
			switch (boxType)
			{
				case BoxType.Normal:
					gm.BreakCombo();
					break;
				case BoxType.Large:
					gm.BreakCombo();
					break;
				case BoxType.Iron:
					gm.AddScore();
					break;
			}

			Destroy(gameObject);
		}
	}
	#endregion
}
