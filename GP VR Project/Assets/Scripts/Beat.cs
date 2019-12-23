using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XellExtraUtils;

public enum Direction {None, Up, Down, Left, Right, Forward, Backward }

public enum BoxColor {None, Red, Blue, Green, Yellow }

public enum BoxType {Normal, Slider, Directional } //Normal is Small Beats, Big requires Continuous Punch, Iron requires Dodging

public class Beat : MonoBehaviour, IPooledObject
{
	public static int hitCount;

	[Header("General Components")]
	[SerializeField] GameManager gm;

	[Header("Beat Attributes")]
	public BoxType boxType;
	public BoxColor boxColor;
	public Direction hitDir;

	[Header("Beat Renderers and Materials")]
	[SerializeField] Renderer beatR;
	[SerializeField] Renderer markerR;
	[SerializeField] Material beatMat;
	[SerializeField] Material markerMat;

	[Header("Beat Spawn Fixed Offsets")]
	[SerializeField] float xOffsetRef = 0.3f; //Magnitude of X Offset from Beat Spawn Pos Transform. Ref as xOffset differs based on Color
	[SerializeField] float xOffset; //Var to use to set the Position when moving the Beat 
	[SerializeField] float yOffset = 0; //Y Offset from Beat Spawn Pos Transform

	[Header("Set Target Time and Position")]
	[SerializeField] double spawnedTime; //Time that Beat Spawns
	[SerializeField] double targetTime; //Time that the Beat is meant to be Hit
	[SerializeField] Vector3 spawnPos; //Position of Beat Spawn Position. Used as Ref to get the Offset from Spawn Position to move the Beat
	[SerializeField] float spawnHitDist; //Total Distance between Spawn Position and Hit Position

	[Header("Hit Threshold")]
	[SerializeField] float hitVelSqrThreshold; //Square Magnitude of Required Velocity to Count as Hit 

	// Update is called once per frame
	void Update()
    {
		MoveBeatPosition();
		if (gm.autoMode) AutoHitBeat();
	}

	#region For Beat Initialisation
	//For Initialisation that will only be called Once
	public void InitialiseBeat(GameManager gm, Vector3 spawnPos, float spawnHitDist)
	{
		if (this.gm == null)
		{
			this.gm = gm;

			beatMat = beatR.material;
			markerMat = markerR.material;

			//Should only need to be called once unless you want to play with Different Spawn Positions
			this.spawnPos = spawnPos; //Get the Spawn Position Transform.position
			this.spawnHitDist = spawnHitDist; //Get the Total Distance between the Spawn and Hit Position

			switch (boxType)
			{
				case BoxType.Normal:
					hitVelSqrThreshold = 25;
					break;
				case BoxType.Directional:
					hitVelSqrThreshold = 25;
					break;
				case BoxType.Slider:
					hitVelSqrThreshold = 20;
					break;
			}
		}
	}

	//Box Type has already been set in Prefabs
	//Set Values to get the Timings and Renderers right
	public void SetBeatInstanceValues(BoxColor color, double spawnedTime, double targetTime, Direction boxDir = Direction.None, bool debugValues = false) // Vector3 spawnPos, float spawnHitDist
	{
		this.spawnedTime = spawnedTime;
		this.targetTime = targetTime / 1000;

		boxColor = color;
		hitDir = boxDir;

		SetColoredBeatValues(color);

		//Change Arrow Angle here as Hit Direction may change based on Color. (Left to Right, Right to Left)
		switch (hitDir)
		{
			case Direction.Left:
				markerR.transform.localEulerAngles = new Vector3(0, 0, 90);
				break;
			case Direction.Down:
				markerR.transform.localEulerAngles = new Vector3(0, 0, 180);
				break;
			case Direction.Right:
				markerR.transform.localEulerAngles = new Vector3(0, 0, 270);
				break;
			default:
				markerR.transform.localEulerAngles = new Vector3(0, 0, 0);
				break;
		}

		if (debugValues) print(string.Format("Beat initialised. Transform is: {0}. Target Time is: {1}. Spawned Time is: {2}. Spawn Position is: {3} Box Color is: {4}", transform.position.z, this.targetTime, this.spawnedTime, spawnPos, boxColor));
	}

	void SetColoredBeatValues(BoxColor color)
	{
		//Left 90 degrees
		switch (color)
		{
			case BoxColor.Red:
				xOffset = -xOffsetRef;
				//if (hitDir == Direction.Left) hitDir = Direction.Right; //Not Needed as Default for Horizontal is Direction.Right

				MaterialUtils.ChangeMaterialColor(beatMat, gm.diffuseColors[0]);
				MaterialUtils.ChangeMaterialEmission(beatMat, gm.emissiveColors[0], gm.emissiveIntensities[0]);

				MaterialUtils.ChangeMaterialColor(markerMat, gm.diffuseColors[0]);
				MaterialUtils.ChangeMaterialEmission(markerMat, gm.emissiveColors[0], gm.emissiveIntensities[0]);
				break;
			case BoxColor.Yellow:
				xOffset = -xOffsetRef;
				//if (hitDir == Direction.Left) hitDir = Direction.Right; //Not Needed as Default for Horizontal is Direction.Right

				MaterialUtils.ChangeMaterialColor(beatMat, gm.diffuseColors[3]);
				MaterialUtils.ChangeMaterialEmission(beatMat, gm.emissiveColors[3], gm.emissiveIntensities[3]);

				MaterialUtils.ChangeMaterialColor(markerMat, gm.diffuseColors[3]);
				MaterialUtils.ChangeMaterialEmission(markerMat, gm.emissiveColors[3], gm.emissiveIntensities[3]);
				break;
			case BoxColor.Blue:
				xOffset = xOffsetRef;
				if (hitDir == Direction.Right) hitDir = Direction.Left;

				MaterialUtils.ChangeMaterialColor(beatMat, gm.diffuseColors[1]);
				MaterialUtils.ChangeMaterialEmission(beatMat, gm.emissiveColors[1], gm.emissiveIntensities[1]);

				MaterialUtils.ChangeMaterialColor(markerMat, gm.diffuseColors[1]);
				MaterialUtils.ChangeMaterialEmission(markerMat, gm.emissiveColors[1], gm.emissiveIntensities[1]);

				
				break;
			case BoxColor.Green:
				xOffset = xOffsetRef;
				if (hitDir == Direction.Right) hitDir = Direction.Left;

				MaterialUtils.ChangeMaterialColor(beatMat, gm.diffuseColors[2]);
				MaterialUtils.ChangeMaterialEmission(beatMat, gm.emissiveColors[2], gm.emissiveIntensities[2]);

				MaterialUtils.ChangeMaterialColor(markerMat, gm.diffuseColors[2]);
				MaterialUtils.ChangeMaterialEmission(markerMat, gm.emissiveColors[2], gm.emissiveIntensities[2]);
				break;
		}
	}

	void ResetBeatValues()
	{
		spawnedTime = 0;
		targetTime = 0;
	}
	#endregion

	#region For Beat Hit
	public void AutoHitBeat()
	{
		if (GetOffsetRatio() >= 1.0f) Hit();
	}

	public void Hit(int score = 300)
	{
		//Play Sound Effect
		gm.AddScore(score);
		GameObject particle = ObjectPooling.inst.SpawnFromPool("Hit Particles", transform.position, Quaternion.identity);
		particle.transform.localScale = transform.localScale;
		ObjectPooling.inst.ReturnToPool(gameObject, GetPoolTag()); //Destroy(gameObject);
	}

	public Direction GetHitDirection(Vector3 hitDir)
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
	}
	#endregion

	#region For Beat Movement
	public void MoveBeatPosition()
	{
		float zOffset = Mathf.LerpUnclamped(0, spawnHitDist, GetOffsetRatio());
		if (float.IsNaN(zOffset)) zOffset = 0;

		transform.position = spawnPos + new Vector3(xOffset, yOffset, zOffset);
	}

	float GetOffsetRatio()
	{
		return (float)((gm.GetTrackTime() - spawnedTime) / (targetTime - spawnedTime));
	}
	#endregion

	#region Pooling Functions
	public void OnObjectSpawn()
	{
		Vector3 pos = transform.position;
		pos.y = 0;
		ObjectPooling.inst.SpawnFromPool("Spawn Despawn Particles", pos, Quaternion.Euler(-90, 0, 0));
	}

	public void OnObjectDespawn()
	{

	}

	public string GetPoolTag()
	{
		switch (boxType)
		{
			case BoxType.Normal:
				return "Normal";
			case BoxType.Slider:
				return "Slider";
			case BoxType.Directional:
				return "Directional";
			default:
				return "Normal";
		}
	}
	#endregion

	#region Forced Despawn
	public void ForceDespawn(bool inDestroyZone = true)
	{
		Vector3 position = transform.position;
		position.y = 0;

		ObjectPooling.inst.SpawnFromPool("Spawn Despawn Particles", position, Quaternion.Euler(-90, 0, 0));
		ObjectPooling.inst.ReturnToPool(gameObject, GetPoolTag());
	}
	#endregion

	#region Trigger Functions
	private void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Player")
		{
			Glove glove = other.GetComponent<Glove>();
			Vector3 hitVel = glove.GetControllerVelocity();

			//print(GetHitDirection(hitVel.normalized));

			if (glove.gloveColor != boxColor || hitVel.sqrMagnitude < hitVelSqrThreshold) return;

			switch (boxType)
			{
				case BoxType.Normal:
					Hit();
					break;
				case BoxType.Slider:
					Hit(50);
					break;
				case BoxType.Directional:
					//Check whether Direction is Correct
					if (hitDir == GetHitDirection(hitVel.normalized)) Hit(); //Check Directional Input
					break;
			}
		}
		else if (other.tag == "Destroy Zone")
		{
			gm.BreakCombo();
			ForceDespawn();
		}
	}
	#endregion
}
