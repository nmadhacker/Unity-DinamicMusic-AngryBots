using UnityEngine;
using System.Collections;

public class WwiseFootstepHandler : MonoBehaviour 
{
	public enum FootType {
		Player,
		Mech,
		Spider
	}

	PhysicMaterial physicMaterial;
	FootType footType;

	void SetFootType(object footType)
	{
		this.footType = (FootType)footType;
	}

	void SetPhysicsMaterial(object physicMaterial)
	{
		this.physicMaterial = physicMaterial as PhysicMaterial;
	}

	void OnFootstep()
	{
		WwiseMaterialImpactManager.TriggerFootstepSound(footType, physicMaterial, this.gameObject);
	}
}
