using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particles : MonoBehaviour, IPooledObject
{
	ParticleSystem pSys;

    // Update is called once per frame
    void Update()
    {
		if (pSys.isStopped) ObjectPooling.inst.ReturnToPool(gameObject, GetPoolTag());
    }

	public void OnObjectSpawn()
	{
		if (!pSys) pSys = GetComponent<ParticleSystem>();
		pSys.Play(true);
	}

	public void OnObjectDespawn()
	{

	}

	public string GetPoolTag()
	{
		return "Particles";
	}
}
