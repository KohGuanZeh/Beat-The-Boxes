using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XellExtraUtils;

public enum BoxColor {None, Red, Blue, Green, Yellow }

public enum BoxType {Normal, Iron, Large } //Normal is Small Beats, Big requires Continuous Punch, Iron requires Dodging

public class Beat : MonoBehaviour, IPooledObject
{
	//0 is Red, 1 is Blue, 2 is Green, 3 is Yellow, 4 is Iron, 5 is Large
	public static int hitCount;

	[Header("General Components")]
	[SerializeField] GameManager gm;
	[SerializeField] float startXDisplaceVal = 0.5f;
	[SerializeField] float finalXDisplaceVal = 0.15f;
	[SerializeField] Transform beatMeshObj;
	[SerializeField] bool initialised;

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
	[SerializeField] Vector3 spawnPos;
	[SerializeField] float spawnHitDist; //Z Displacement
	[SerializeField] float xDisplacement; //X Displacement

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

	public void InitialiseBeat(double spawnedTime, double targetTime, Vector3 spawnPos, float spawnHitDist)
	{
		beatMeshObj = transform.GetChild(0);

		this.spawnedTime = spawnedTime;
		this.targetTime = targetTime / 1000;

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

		//X Rotation should always be Negative. Y and Z rotation (+/-) will be random
		rotation = new Vector3(-Random.Range(minRot, maxRot),
								MathFunctions.RandomisePositiveNegative() * Random.Range(minRot, maxRot),
								MathFunctions.RandomisePositiveNegative() * Random.Range(minRot, maxRot));

		initialised = true;

		print(string.Format("Beat initialised. Transform is: {0}. Target Time is: {1}. Spawned Time is: {2}. Spawn Position is: {3} Box Color is: {4}", transform.position.z, this.targetTime, this.spawnedTime, this.spawnPos, boxColor));
	}

	void UninitialiseBeat()
	{
		spawnedTime = 0;
		targetTime = 0;

		spawnPos = Vector3.zero;
		spawnHitDist = 0;

		xDisplacement = 0;
	}

	void SetDisplacement(bool isPositive)
	{
		if (isPositive) xDisplacement = startXDisplaceVal;
		else xDisplacement = -startXDisplaceVal;
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
					Hit();
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
		ObjectPooling.inst.SpawnFromPool("Particles", transform.position, Quaternion.identity);
		float zOffset = Mathf.LerpUnclamped(0, spawnHitDist, GetOffsetRatio());
		print(string.Format("Offset is: {0}, Transform is: {1}. HitCount is {2}. Target Time is: {3}. Spawned Time is: {4}. Spawn Position is: {5} Box Color is: {6}", zOffset, transform.position.z, ++hitCount, targetTime, spawnedTime, spawnPos, boxColor));
		if (destroy) ObjectPooling.inst.ReturnToPool(gameObject, GetPoolTag());
	}
	#endregion

	#region For Beat Movement
	public void MoveBeatPosition()
	{
		float zOffset = Mathf.LerpUnclamped(0, spawnHitDist, GetOffsetRatio());
		if (float.IsNaN(zOffset))
		{
			print("Nan Detected");
			zOffset = 0;
		}
		float xOffset = xDisplacement;
		float yOffset = 0;
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

	#region Pooling Functions
	public void OnObjectSpawn()
	{
		
	}

	public void OnObjectDespawn()
	{
		initialised = false;
	}

	public string GetPoolTag()
	{
		switch (boxColor)
		{
			case BoxColor.Red:
				return "Red";
			case BoxColor.Yellow:
				return "Yellow";
			case BoxColor.Blue:
				return "Blue";
			case BoxColor.Green:
				return "Green";
			default:
				return "None";
		}
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

			ObjectPooling.inst.ReturnToPool(gameObject, GetPoolTag());
		}
	}
	#endregion
}
