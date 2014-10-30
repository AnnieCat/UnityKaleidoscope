using UnityEngine;
using System.Collections;

public class LoadLevel : MonoBehaviour {

	//public int totalLevels = 3;
	//private int currentLevel= 0;
	
	public Texture screenUI;
	
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
		/*if(Input.GetKey(KeyCode.Alpha5))
		{
			if(currentLevel+1<totalLevels)
				Application.LoadLevel(currentLevel+1);
			else
				Application.LoadLevel(0);
		}*/
		
		if(Input.GetKey(KeyCode.Alpha5))
		{
			if(Application.loadedLevel == 0)
				Application.LoadLevel(1);
			if(Application.loadedLevel == 1 || Application.loadedLevel==2 ||Application.loadedLevel==3)
				Application.LoadLevel(4);
			if(Application.loadedLevel==4 || Application.loadedLevel==5 || Application.loadedLevel == 6)
				Application.LoadLevel(0);
		}
		
		if(Input.GetKey(KeyCode.Alpha3) && Application.loadedLevel!=0)
		{
			if(Application.loadedLevel==1)
				Application.LoadLevel(2);
			if(Application.loadedLevel==2)
				Application.LoadLevel(3);
			if(Application.loadedLevel==3)
				Application.LoadLevel(1);
			
			/*if(Application.loadedLevel==4)
				Application.LoadLevel(5);
			if(Application.loadedLevel==5)
				Application.LoadLevel(6);
			if(Application.loadedLevel==6)
				Application.LoadLevel(4);*/
		}
		
	
	}
	
	void OnGUI()
		{
//			GUI.DrawTexture(new Rect(0,0,720,1280), screenUI, ScaleMode.ScaleToFit, true, 10.0f);
		GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height), screenUI);
	}
}
