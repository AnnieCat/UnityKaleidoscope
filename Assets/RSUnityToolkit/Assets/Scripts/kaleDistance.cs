using UnityEngine;
using System.Collections;

public class kaleDistance : MonoBehaviour {

	public GameObject	myHead;
	public Animator[]	myAnims;
	
	private int		currentPos;
	private int		currentNum;
	private bool	changed = false;
	
	void Start(){
		
		currentNum = Random.Range(0,3);
		
	}
	
	void Update () {
		
		StartCoroutine(FrameCheck());
	
		if(myHead.transform.position.z<45f)
		{
			currentPos = 1;
			if(changed)
			{
				currentNum = Random.Range (0,3);
				foreach(Animator x in myAnims)
					x.SetInteger("distance",currentNum);
			}
		}
		if(myHead.transform.position.z>45f && myHead.transform.position.z <57f )
		{
			currentPos = 2;
			if(changed)
			{
				currentNum = Random.Range (0,3);
				foreach(Animator x in myAnims)
					x.SetInteger("distance",currentNum);
			}
		}
		if(myHead.transform.position.z>57f && myHead.transform.position.z < 69f )
		{
			currentPos = 3;
			if(changed)
			{
				currentNum = Random.Range (0,3);
				foreach(Animator x in myAnims)
					x.SetInteger("distance",currentNum);
			}
		}
		if(myHead.transform.position.z>69f && myHead.transform.position.z < 80f)
		{
			currentPos = 4;
			if(changed)
			{
				currentNum = Random.Range (0,3);
				foreach(Animator x in myAnims)
					x.SetInteger("distance",currentNum);
			}
		}		
	}
	
	IEnumerator FrameCheck(){
		int lastFrame = currentPos;
		yield return new WaitForEndOfFrame();
		int thisFrame = currentPos;
		
		if (lastFrame!=thisFrame)
		{
			changed = true;
			yield return new WaitForEndOfFrame();
			changed = false;
		}
	}
}
