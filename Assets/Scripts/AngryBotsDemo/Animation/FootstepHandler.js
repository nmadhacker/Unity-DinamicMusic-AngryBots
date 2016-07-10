#pragma strict

enum FootType {
	Player,
	Mech,
	Spider
}

var audioSource : AudioSource;
var footType : FootType;

private var physicMaterial : PhysicMaterial;
private var msgReceiver : GameObject;

function Awake()
{
	msgReceiver = new GameObject("receiver");
	UnityEngineInternal.APIUpdaterRuntimeServices.AddComponent(msgReceiver, "Assets/Scripts/AngryBotsDemo/Animation/FootstepHandler.js(18,9)", "WwiseFootstepHandler");
	msgReceiver.transform.SetParent(this.gameObject.transform,false);
	msgReceiver.SendMessage("SetFootType",footType, SendMessageOptions.DontRequireReceiver);
}

function OnCollisionEnter (collisionInfo : Collision) {
	if (physicMaterial != collisionInfo.collider.sharedMaterial)
	{
		physicMaterial = collisionInfo.collider.sharedMaterial;

		if (physicMaterial != null)
		{
			msgReceiver.SendMessage("SetPhysicsMaterial",physicMaterial,SendMessageOptions.DontRequireReceiver);
		}
	}
}

function OnFootstep () {
	if (!audioSource.enabled)
	{
		return;
	}

	msgReceiver.SendMessage("OnFootstep",SendMessageOptions.DontRequireReceiver);
	
	var sound : AudioClip;
	switch (footType) {
	case FootType.Player:
		sound = MaterialImpactManager.GetPlayerFootstepSound (physicMaterial);
		break;
	case FootType.Mech:
		sound = MaterialImpactManager.GetMechFootstepSound (physicMaterial);
		break;
	case FootType.Spider:
		sound = MaterialImpactManager.GetSpiderFootstepSound (physicMaterial);
		break;
	}	
	audioSource.pitch = Random.Range (0.98, 1.02);
	audioSource.PlayOneShot (sound, Random.Range (0.8, 1.2));
}
