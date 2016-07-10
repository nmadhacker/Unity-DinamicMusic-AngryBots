using UnityEngine;
using System.Collections;

public class WwiseAutoPlayStopSfx : MonoBehaviour {

	public string switchGroup;
	public string switchKey;

	public string playEvent;
	public string stopEvent;

	void OnPlay()
	{
		UpdateSwitch();
		AkSoundEngine.PostEvent(playEvent,this.gameObject);
	}

	void OnStop()
	{
		UpdateSwitch();
		AkSoundEngine.PostEvent(stopEvent, this.gameObject);
	}

	void UpdateSwitch()
	{
		if (!string.IsNullOrEmpty(switchGroup) && !string.IsNullOrEmpty(switchKey))
		{
			AkSoundEngine.SetSwitch(switchGroup,switchKey,this.gameObject);
		}
	}
}
