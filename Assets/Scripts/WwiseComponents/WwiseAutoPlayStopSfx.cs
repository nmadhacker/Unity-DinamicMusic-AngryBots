using UnityEngine;
using System.Collections;

public class WwiseAutoPlayStopSfx : MonoBehaviour {

	public string switchGroup;
	public string switchKey;

	public string playEvent;
	public string stopEvent;

	void OnPlay()
	{
		//UpdateSwitch();
		//AkSoundEngine.PostEvent(playEvent,this.gameObject);
		// using event for everything!!!
		string eventToPlay = string.Format ("{0}_{1}_{2}", playEvent, switchGroup, switchKey);
		AkSoundEngine.PostEvent (eventToPlay, this.gameObject);
	}

	void OnStop()
	{
		//UpdateSwitch();
		//AkSoundEngine.PostEvent(stopEvent, this.gameObject);
		// using event for everything!!!
		string eventToPlay = string.Format ("{0}_{1}_{2}", stopEvent, switchGroup, switchKey);
		AkSoundEngine.PostEvent (eventToPlay, this.gameObject);
	}

	void UpdateSwitch()
	{
		if (!string.IsNullOrEmpty(switchGroup) && !string.IsNullOrEmpty(switchKey))
		{
			AkSoundEngine.SetSwitch(switchGroup,switchKey,this.gameObject);
		}
	}
}
