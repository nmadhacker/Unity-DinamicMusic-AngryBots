using UnityEngine;
using System.Collections;

public class WwiseBulletHitHandler : MonoBehaviour {
	
	PhysicMaterial physicMaterial;
	GameObject source;

	void SetPhysicsMaterial(object physicMaterial)
	{
		this.physicMaterial = physicMaterial as PhysicMaterial;
	}

	void SetSource(GameObject source)
	{
		this.source = source;
	}

	void OnBulletHit()
	{
		WwiseMaterialImpactManager.TriggerBulletHitSound(physicMaterial, this.source);
	}
}
