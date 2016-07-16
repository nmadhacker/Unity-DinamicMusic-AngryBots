using UnityEngine;
using System.Collections;

public class WwiseGamerScoreUpdater : MonoBehaviour {

	public float enemyCount;
	public string gamerScoreKey;

	void OnUpdateGamerScore()
	{
		int currentScore = Mathf.FloorToInt(GameOverGUI.GetCurrentPlayerKills());
		float percent = currentScore * 100f / enemyCount;
		AkSoundEngine.SetRTPCValue(gamerScoreKey,percent);
	}
}
