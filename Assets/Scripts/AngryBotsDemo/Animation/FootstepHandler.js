#pragma strict

enum FootType {
	Player,
	Mech,
	Spider
}

var footType : FootType;

private var physicMaterial : PhysicMaterial;
var msgReceiver : GameObject;

function Awake()
{
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
	msgReceiver.SendMessage("OnFootstep",SendMessageOptions.DontRequireReceiver);
}
