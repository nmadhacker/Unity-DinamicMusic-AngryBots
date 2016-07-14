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
	public string sndKey;
}

public class WwiseMaterialImpactManager : MonoBehaviour
{
	[Header("Entities Footstep keys")]
	public WwiseEntityFootstepSndConfig[] footstepEntityKeys;
	public WwiseFootstepMaterialImpact[] footstepMaterials;

	[Header("Bullet Hits Keys")]
	public string bulletHitEventKey;
	public WwiseBulletHitImpact[] bulletHitMaterials;

	private static Dictionary<PhysicMaterial, string> footSndDict;
	private static Dictionary<PhysicMaterial, string> bulletHitDict;
	private static Dictionary<WwiseFootstepHandler.FootType,string> entityDict; 

	private static WwiseFootstepMaterialImpact defaultMat;
	private static WwiseEntityFootstepSndConfig defaultEntity;
	private static WwiseBulletHitImpact defaultImpact;
	private static string static_bulletHitEventKey;

	void Awake()
	{
		defaultMat = footstepMaterials[0];
		defaultEntity = footstepEntityKeys[0];
		defaultImpact = bulletHitMaterials[0];
		static_bulletHitEventKey = bulletHitEventKey;

		footSndDict = new Dictionary<PhysicMaterial, string> ();
		for (var i = 0; i < footstepMaterials.Length; i++) {
			footSndDict[footstepMaterials[i].physicMaterial] = footstepMaterials[i].sndKey;
		}

		entityDict = new Dictionary<WwiseFootstepHandler.FootType, string>();
		for(var i = 0; i < footstepEntityKeys.Length; ++i) {
			entityDict[footstepEntityKeys[i].footType] = footstepEntityKeys[i].sndEventKey;
		}

		bulletHitDict = new Dictionary<PhysicMaterial, string>();
		for(var i = 0; i < bulletHitMaterials.Length; ++i ) {
			bulletHitDict[bulletHitMaterials[i].physicMaterial] = bulletHitMaterials[i].sndKey;
		}
	}

	public static void TriggerFootstepSound(WwiseFootstepHandler.FootType footType, PhysicMaterial mat, GameObject source)
	{
		string eventKey = GetEntityEventKey(footType);
		string switchKey = GeFootsteptMaterialImpact(mat);
		//AkSoundEngine.SetSwitch("material",switchKey,source);
		//AkSoundEngine.PostEvent(eventKey,source);
		// using event for everything!!!
		string eventName = string.Format("{0}_{1}_{2}",eventKey,switchKey,footType.ToString() );
		AkSoundEngine.PostEvent (eventName, source);
	}

	public static void TriggerBulletHitSound(PhysicMaterial mat, GameObject source)
	{
		string switchKey = GetBulletImpactMaterialImpact(mat);
		//AkSoundEngine.SetSwitch("material",switchKey,source);
		//AkSoundEngine.PostEvent(static_bulletHitEventKey,source);
		// using event for everything!!!
		string eventName = string.Format("{0}_{2}",static_bulletHitEventKey,switchKey);
		AkSoundEngine.PostEvent (eventName, source);
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

	static string GetBulletImpactMaterialImpact(PhysicMaterial mat)
	{
		if (mat && bulletHitDict.ContainsKey(mat))
			return bulletHitDict[mat];
		return defaultImpact.sndKey;
	}
}
