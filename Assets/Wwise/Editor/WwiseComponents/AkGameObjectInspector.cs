#if UNITY_EDITOR
//////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2014 Audiokinetic Inc. / All Rights Reserved
//
//////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Reflection;

public class DefaultHandles
{
	public static bool Hidden
	{
		get
		{
			Type type = typeof(Tools);
			FieldInfo field = type.GetField("s_Hidden", BindingFlags.NonPublic | BindingFlags.Static);
			return ((bool)field.GetValue(null));
		}
		set
		{
			Type type = typeof(Tools);
			FieldInfo field = type.GetField("s_Hidden", BindingFlags.NonPublic | BindingFlags.Static);
			field.SetValue(null, value);
		}
	}
}

[CanEditMultipleObjects]
[CustomEditor(typeof(AkGameObj))]
public class AkGameObjectInspector : Editor
{
	AkGameObj m_AkGameObject;

	bool hideDefaultHandle = false;

	void OnEnable()
	{
		m_AkGameObject = target as AkGameObj;

		DefaultHandles.Hidden = hideDefaultHandle;
	}

	void OnDisable()
	{
		DefaultHandles.Hidden = false;
	}

	public override void OnInspectorGUI()
	{
		// Unity tries to construct a AkGameObjPositionOffsetData all the time. Need this ugly workaround
		// to prevent it from doing this.
		if (m_AkGameObject.m_positionOffsetData != null)
		{
			if (!m_AkGameObject.m_positionOffsetData.KeepMe)
			{
				m_AkGameObject.m_positionOffsetData = null;
			}
		}

		AkGameObjPositionOffsetData positionOffsetData = m_AkGameObject.m_positionOffsetData;
		Vector3 positionOffset = Vector3.zero;

#if UNITY_5_3_OR_NEWER
		EditorGUI.BeginChangeCheck();
#endif

		GUILayout.BeginVertical("Box");

		bool applyPosOffset = EditorGUILayout.Toggle("Apply Position Offset:", positionOffsetData != null);

		if (applyPosOffset != (positionOffsetData != null))
		{
			positionOffsetData = applyPosOffset ? new AkGameObjPositionOffsetData(true) : null;
		}

		if (positionOffsetData != null)
		{
			positionOffset = EditorGUILayout.Vector3Field("Position Offset", positionOffsetData.positionOffset);

			GUILayout.Space(2);

			if (hideDefaultHandle)
			{
				if (GUILayout.Button("Show Main Transform"))
				{
					hideDefaultHandle = false;
					DefaultHandles.Hidden = hideDefaultHandle;
				}
			}
			else if (GUILayout.Button("Hide Main Transform"))
			{
				hideDefaultHandle = true;
				DefaultHandles.Hidden = hideDefaultHandle;
			}
		}
		else if (hideDefaultHandle)
		{
			hideDefaultHandle = false;
			DefaultHandles.Hidden = hideDefaultHandle;
		}

		GUILayout.EndVertical();

		GUILayout.Space(3);

		GUILayout.BeginVertical("Box");

		bool isEnvironmentAware = EditorGUILayout.Toggle("Environment Aware:", m_AkGameObject.isEnvironmentAware);

		if (isEnvironmentAware && m_AkGameObject.GetComponent<Rigidbody>() == null)
		{
			GUIStyle style = new GUIStyle();
			style.normal.textColor = Color.red;
			style.wordWrap = true;
			GUILayout.Label("Objects affected by Environment need to have a RigidBody attached.", style);
			if (GUILayout.Button("Add Rigidbody!"))
			{
				Rigidbody rb = m_AkGameObject.gameObject.AddComponent<Rigidbody>();
				rb.useGravity = false;
				rb.isKinematic = true;
			}
		}

		GUILayout.EndVertical();

		GUILayout.Space(3);

		string[] maskLabels = new string[AkSoundEngine.AK_NUM_LISTENERS];
		for (int i = 0; i < AkSoundEngine.AK_NUM_LISTENERS; i++)
		{
			maskLabels[i] = "L" + i;
		}

		int listenerMask = EditorGUILayout.MaskField("Listeners", m_AkGameObject.listenerMask, maskLabels);

#if UNITY_5_3_OR_NEWER
		if (EditorGUI.EndChangeCheck())
#else
		if (GUI.changed)
#endif
		{
#if UNITY_5_3_OR_NEWER
			Undo.RecordObject(target, "AkGameObj Parameter Change");
#endif
			m_AkGameObject.m_positionOffsetData = positionOffsetData;

			if (positionOffsetData != null)
				m_AkGameObject.m_positionOffsetData.positionOffset = positionOffset;

			m_AkGameObject.isEnvironmentAware = isEnvironmentAware;
			m_AkGameObject.listenerMask = listenerMask;

#if !UNITY_5_3_OR_NEWER
			EditorUtility.SetDirty(m_AkGameObject);
#endif
		}
	}

	void OnSceneGUI()
	{
		if (m_AkGameObject.m_positionOffsetData == null)
			return;

#if UNITY_5_3_OR_NEWER
		EditorGUI.BeginChangeCheck();
#endif

		// Transform local offset to world coordinate
		Vector3 pos = m_AkGameObject.transform.TransformPoint(m_AkGameObject.m_positionOffsetData.positionOffset);

		// Get new handle position
		pos = Handles.PositionHandle(pos, Quaternion.identity);

#if UNITY_5_3_OR_NEWER
		if (EditorGUI.EndChangeCheck())
#else
		if (GUI.changed)
#endif
		{
#if UNITY_5_3_OR_NEWER
			Undo.RecordObject(target, "Position Offset Change");
#endif

			// Transform wolrd offset to local coordintae
			m_AkGameObject.m_positionOffsetData.positionOffset = m_AkGameObject.transform.InverseTransformPoint(pos);

#if !UNITY_5_3_OR_NEWER
			EditorUtility.SetDirty(target);
#endif
		}
	}
}
#endif