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
	}

	public static ObjectPooling inst;
	[SerializeField] public List<Pool> pools;
	public Dictionary<string, Queue<GameObject>> poolDictionary;
	[SerializeField] Transform parentTransform;

	void Awake()
	{
		inst = this;
	}

	// Use this for initialization
	void Start ()
	{
		poolDictionary = new Dictionary<string, Queue<GameObject>>();
		if (!parentTransform) parentTransform = transform;

		foreach (Pool pool in pools)
		{
			Queue<GameObject> objectPool = new Queue<GameObject>();
			for (int i = 0; i < pool.poolAmt; i++)
			{
				GameObject createdObj = Instantiate(pool.prefab, parentTransform);
				createdObj.SetActive(false);
				objectPool.Enqueue(createdObj);
			}
			poolDictionary.Add(pool.tag, objectPool);
		}
	}

	public GameObject SpawnFromPool(string tag, Vector3 spawnPos, Quaternion spawnRot)
	{
		if (!poolDictionary.ContainsKey(tag))
		{
			Debug.LogWarning("Pool with tag '" + tag + "' does not exist.");
			return null;
		}

		GameObject objToSpawn = poolDictionary[tag].Count > 0 ? poolDictionary[tag].Dequeue() : Instantiate(GetPool(tag).prefab, parentTransform);

		objToSpawn.transform.position = spawnPos;
		objToSpawn.transform.rotation = spawnRot;
		objToSpawn.SetActive(true);

		IPooledObject pooledObject = objToSpawn.GetComponent<IPooledObject>();
		if (pooledObject != null) pooledObject.OnObjectSpawn();

		poolDictionary[tag].Enqueue(objToSpawn);
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
