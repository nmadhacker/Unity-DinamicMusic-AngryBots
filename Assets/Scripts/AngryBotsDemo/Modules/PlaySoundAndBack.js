#pragma strict

var audioSource : AudioSource;
var sound : AudioClip;
var soundReverse : AudioClip;
var lengthWithoutTrailing : float = 0;

private var back : boolean = false;
private var normalizedTime : float = 0;
var wwiseReceiver : GameObject;

function Awake () {
	if (!audioSource && GetComponent.<AudioSource>())
		audioSource = GetComponent.<AudioSource>();
	if (lengthWithoutTrailing == 0)
		lengthWithoutTrailing = Mathf.Min (sound.length, soundReverse.length);
}

function OnSignal () {		
	PlayWithDirection ();
}

function OnPlay () {	
	// Set the speed to be positive
	back = false;
	PlayWithDirection ();
}

function OnPlayReverse () {			
	back = true;
	PlayWithDirection ();
}

private function PlayWithDirection () {
	
	var playbackTime : float;
	
	if (back) {
		audioSource.clip = soundReverse;
		playbackTime = (1 - normalizedTime) * lengthWithoutTrailing;
	}
	else {
		audioSource.clip = sound;
		playbackTime = normalizedTime * lengthWithoutTrailing;
	}

	if (wwiseReceiver != null)
	{
		if (!back){ wwiseReceiver.SendMessage("OnPlay"); }
		else { wwiseReceiver.SendMessage("OnStop"); }
	}
	
	//audioSource.time = playbackTime;
	//audioSource.Play ();
	
	back = !back;
}
