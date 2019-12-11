using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooling : MonoBehaviour
{
	[System.Serializable] //Identifier to know what prefab to spawn
	public class Pool
	{
		public string tag;
		public GameObject prefab;
		public int poolAmt;
		public Transform parentObj;
	}

	public static ObjectPooling inst;
	[SerializeField] public List<Pool> pools;
	public Dictionary<string, Queue<GameObject>> poolDictionary;

	void Awake()
	{
		inst = this;

		poolDictionary = new Dictionary<string, Queue<GameObject>>();

		foreach (Pool pool in pools)
		{
			Queue<GameObject> objectPool = new Queue<GameObject>();
			for (int i = 0; i < pool.poolAmt; i++)
			{
				GameObject createdObj = Instantiate(pool.prefab, pool.parentObj);
				createdObj.SetActive(false);
				objectPool.Enqueue(createdObj);
			}
			poolDictionary.Add(pool.tag, objectPool);
		}
	}

	public GameObject SpawnFromPool(string tag, Vector3 spawnPos, Quaternion spawnRot, Transform parent = null)
	{
		if (!poolDictionary.ContainsKey(tag))
		{
			Debug.LogWarning("Pool with tag '" + tag + "' does not exist.");
			return null;
		}

		GameObject objToSpawn = poolDictionary[tag].Count > 0 ? poolDictionary[tag].Dequeue() : Instantiate(GetPool(tag).prefab, GetPool(tag).parentObj);

		objToSpawn.transform.position = spawnPos;
		objToSpawn.transform.rotation = spawnRot;
		if (parent) objToSpawn.transform.parent = parent; //If Parent is not Null, Set New Parent
		objToSpawn.SetActive(true);

		IPooledObject pooledObject = objToSpawn.GetComponent<IPooledObject>();
		if (pooledObject != null) pooledObject.OnObjectSpawn();

		//poolDictionary[tag].Enqueue(objToSpawn); Like why...
		return objToSpawn;
	}

	public void ReturnToPool(GameObject obj, string tag)
	{
		if (!poolDictionary.ContainsKey(tag))
		{
			Debug.LogWarning("Pool with tag '" + tag + "' does not exist.");
			return;
		}

		obj.SetActive(false);
		IPooledObject pooledObject = obj.GetComponent<IPooledObject>();
		if (pooledObject != null) pooledObject.OnObjectDespawn();
		poolDictionary[tag].Enqueue(obj);
	}

	public Pool GetPool(string tag)
	{
		return pools.Find(x => x.tag == tag);
	}
}


public interface IPooledObject
{
	void OnObjectSpawn();
	void OnObjectDespawn();
	string GetPoolTag();
}
