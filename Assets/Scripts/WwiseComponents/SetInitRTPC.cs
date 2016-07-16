using UnityEngine;
using System.Collections;

public class SetInitRTPC : MonoBehaviour {

	// Use this for initialization
	void Start () {

  
    AkSoundEngine.SetRTPCValue("Intensity", 0);
    AkSoundEngine.SetRTPCValue("Player_Kills_Progress", 0);


    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
