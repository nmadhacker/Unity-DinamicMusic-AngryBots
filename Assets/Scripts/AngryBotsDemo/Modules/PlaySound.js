#pragma strict

var audioSource : AudioSource;
var sound : AudioClip;

var wwiseReceiver : GameObject;

function Awake () {
	if (!audioSource && GetComponent.<AudioSource>())
		audioSource = GetComponent.<AudioSource>();
}

function OnSignal () {

	if (wwiseReceiver != null) { wwiseReceiver.SendMessage("OnPlay"); }

	if (sound)
		audioSource.clip = sound;
	audioSource.Play ();
}