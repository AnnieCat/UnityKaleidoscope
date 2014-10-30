using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ZoomControl : MonoBehaviour
{
    public GameObject Zoomer;
    public List<GameObject> ZoomParticles;
    public int SpawnTime;

    private int mCounter;
	void Start ()
    {
        mCounter = 0;
        Spawn();
	}
	
	void Update ()
    {
        mCounter += 1;
        if( (mCounter%SpawnTime)==0 )
        {
            mCounter = 0;
            Spawn();
        }

        for (int i = ZoomParticles.Count - 1; i >= 0;--i )
        {
            if (ZoomParticles[i].GetComponent<ZoomParticle>().IsDead())
            {
                GameObject.Destroy(ZoomParticles[i]);
                ZoomParticles.RemoveAt(i);
            }
        }
	}

    void Spawn()
    {
        GameObject cZP = Instantiate(Zoomer, transform.position, Quaternion.identity) as GameObject;
        ZoomParticles.Add(cZP);
    }

}
