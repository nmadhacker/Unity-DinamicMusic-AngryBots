using UnityEngine;
using System.Collections;

public class WwiseGamerScoreUpdater : MonoBehaviour {

	public string gamerScoreKey;
	void OnUpdateGamerScore()
	{
		int currentScore = Mathf.FloorToInt(GameOverGUI.GetCurrentScore());
		AkSoundEngine.SetRTPCValue(gamerScoreKey,currentScore);
	}
}
