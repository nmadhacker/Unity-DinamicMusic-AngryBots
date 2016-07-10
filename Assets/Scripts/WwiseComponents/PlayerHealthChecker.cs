﻿using UnityEngine;
using System.Collections;

public class PlayerHealthChecker : MonoBehaviour {

	float maxHp = 1; // initial value
	public string GameParameter = "Health";
	public bool invertValue = false;

	private void SetMaxHp(float maxHp)
	{
		this.maxHp = maxHp;
	}

	private void SetCurrentHp(float currentHp)
	{
		float percent = currentHp/maxHp;
		percent *= invertValue ? -1 : 1;
		AkSoundEngine.SetRTPCValue(GameParameter,percent);
	}
}
