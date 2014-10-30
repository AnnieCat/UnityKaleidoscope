using UnityEngine;
using System.Collections;

public class MirrorDistance : MonoBehaviour {
	
	public GameObject	myHead;
	
	public Animator	myAnim;
	
	public int tempNum;
	
	void Start(){
		
		myAnim.SetInteger("distance",5);
		
	}
	
	void Update () {
	
		if(myHead.transform.position.z<45f)
			myAnim.SetInteger("distance",1);
		if(myHead.transform.position.z>45f && myHead.transform.position.z <57f )
			myAnim.SetInteger("distance",2);
		if(myHead.transform.position.z>57f && myHead.transform.position.z < 69f )
			myAnim.SetInteger("distance",3);
		if(myHead.transform.position.z>69f && myHead.transform.position.z < 80f)
			myAnim.SetInteger("distance",4);
		if(Input.GetKey(KeyCode.Alpha5))
			myAnim.SetInteger("distance",5);
		
		
		print("X: " + myHead.transform.position.x + "Y: " + myHead.transform.position.y + "Z: " + myHead.transform.position.z);
	}
}