using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class WwiseFootstepMaterialImpact {
	public PhysicMaterial physicMaterial;
	public string sndKey;
}

[System.Serializable]
public class WwiseEntityFootstepSndConfig
{
	public WwiseFootstepHandler.FootType footType;
	public string sndEventKey;
}

[System.Serializable]
public class WwiseBulletHitImpact
{
	public PhysicMaterial physicMaterial;
	public string bulletSndKey;
	public string rocketSndKey;
}

public class WwiseMaterialImpactManager : MonoBehaviour
{
	[Header("Entities Footstep keys")]
	public WwiseEntityFootstepSndConfig[] footstepEntityKeys;
	public WwiseFootstepMaterialImpact[] footstepMaterials;
	public WwiseBulletHitImpact[] bulletHitMaterials;

	private static Dictionary<PhysicMaterial, string> footSndDict;
	private static Dictionary<WwiseFootstepHandler.FootType,string> entityDict; 

	private static WwiseFootstepMaterialImpact defaultMat;
	private static WwiseEntityFootstepSndConfig defaultEntity;

	void Awake()
	{
		defaultMat = footstepMaterials[0];
		defaultEntity = footstepEntityKeys[0];

		footSndDict = new Dictionary<PhysicMaterial, string> ();
		for (var i = 0; i < footstepMaterials.Length; i++) {
			footSndDict[footstepMaterials[i].physicMaterial] = footstepMaterials[i].sndKey;
		}

		entityDict = new Dictionary<WwiseFootstepHandler.FootType, string>();
		for(var i = 0; i < footstepEntityKeys.Length; ++i) {
			entityDict[footstepEntityKeys[i].footType] = footstepEntityKeys[i].sndEventKey;
		}
	}

	public static void TriggerFootstepSound(WwiseFootstepHandler.FootType footType, PhysicMaterial mat, GameObject source)
	{
		string eventKey = GetEntityEventKey(footType);
		string switchKey = GeFootsteptMaterialImpact(mat);
		AkSoundEngine.SetSwitch("Footstep_material",switchKey,source);
		AkSoundEngine.PostEvent(eventKey,source);
	}

	static string GetEntityEventKey(WwiseFootstepHandler.FootType entityType)
	{
		if (entityDict.ContainsKey(entityType)){
			return entityDict[entityType];
		}
		return defaultEntity.sndEventKey;
	}

	static string GeFootsteptMaterialImpact (PhysicMaterial mat) 
	{
		if (mat && footSndDict.ContainsKey (mat))
			return footSndDict[mat];
		return defaultMat.sndKey;
	}
}
