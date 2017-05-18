#if UNITY_EDITOR
//////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2014 Audiokinetic Inc. / All Rights Reserved
//
//////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;

public static class AkWwiseProjectInfo
{
    public static AkWwiseProjectData m_Data;

    public static AkWwiseProjectData GetData()
    {
        if (m_Data == null && Directory.Exists(Path.Combine(Application.dataPath, "Wwise")))
        {
			try
			{
				m_Data = (AkWwiseProjectData)AssetDatabase.LoadAssetAtPath("Assets/Wwise/Editor/ProjectData/AkWwiseProjectData.asset", typeof(AkWwiseProjectData));

				if (m_Data == null)
				{
                    if (!Directory.Exists(Path.Combine(Application.dataPath, "Wwise/Editor/ProjectData")))
                    {
                        Directory.CreateDirectory(Path.Combine(Application.dataPath, "Wwise/Editor/ProjectData"));
                    }

					m_Data = ScriptableObject.CreateInstance<AkWwiseProjectData>();
					string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath("Assets/Wwise/Editor/ProjectData/AkWwiseProjectData.asset");
					AssetDatabase.CreateAsset(m_Data, assetPathAndName);
				}
			}
			catch( Exception e )
			{
				Debug.Log("WwiseUnity: Unable to load Wwise Data: " + e.ToString());
			}
        }
		
        return m_Data;
    }


    public static bool Populate()
    {
		bool bDirty = false;
        if (AkWwisePicker.WwiseProjectFound)
		{
			bDirty = AkWwiseWWUBuilder.Populate();
			bDirty |= AkWwiseXMLBuilder.Populate();			
			if(bDirty)
	        {            
	    		EditorUtility.SetDirty(AkWwiseProjectInfo.GetData ());	            
			}
		}
        return bDirty;
    }    
}
#endif