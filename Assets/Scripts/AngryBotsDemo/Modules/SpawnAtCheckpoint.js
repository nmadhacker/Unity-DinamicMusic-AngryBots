#pragma strict
#pragma downcast

var checkpoint : Transform;

var wwiseReceiver : GameObject;

function OnSignal () {
	transform.position = checkpoint.position;
	transform.rotation = checkpoint.rotation;
	
	ResetHealthOnAll ();
	wwiseReceiver.SendMessage("OnPlay");
}

static function ResetHealthOnAll () {
	var healthObjects : Health[] = FindObjectsOfType (Health);
	for (var health : Health in healthObjects) {
		health.dead = false;
		health.SetHealthValue(health.maxHealth);
	}
}
