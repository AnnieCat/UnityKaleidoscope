using UnityEngine;
using System.Collections;

public class ZoomParticle : MonoBehaviour
{
    public float Speed;
    private bool mIsDead;

    public bool IsDead() { return mIsDead; }
	void Start ()
    {
        mIsDead = false;
	}
	
	void Update ()
    {
        Vector3 nP = new Vector3(0, 0, transform.position.z - Speed);
        transform.position = nP;
        float cAlpha = Mathf.Lerp(0, 1, nP.z / 10.0f);
        foreach(Renderer m in GetComponentsInChildren<MeshRenderer>())
        {
            m.material.color = new Color(1, 1, 1, cAlpha);
        }

        if(transform.position.z <= 0)
        {
            mIsDead = true;
        }
	}
}
