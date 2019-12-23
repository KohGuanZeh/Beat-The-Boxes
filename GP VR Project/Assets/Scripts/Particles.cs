using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particles : MonoBehaviour, IPooledObject
{
	[SerializeField] ParticleSystem pSys;
	[SerializeField] AudioSource audio;
	[SerializeField] string poolTag;

    // Update is called once per frame
    void Update()
    {
		if (pSys.isStopped) ObjectPooling.inst.ReturnToPool(gameObject, GetPoolTag());
    }

	public void OnObjectSpawn()
	{
		if (!pSys) pSys = GetComponent<ParticleSystem>();
		if (!audio) audio = GetComponent<AudioSource>();
		pSys.Play(true);
		audio.Play();
	}

	public void OnObjectDespawn()
	{

	}

	public string GetPoolTag()
	{
		return poolTag;
	}
}
