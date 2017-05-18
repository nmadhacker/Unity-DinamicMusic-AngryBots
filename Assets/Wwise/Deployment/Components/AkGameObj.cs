#if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.
//////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2014 Audiokinetic Inc. / All Rights Reserved
//
//////////////////////////////////////////////////////////////////////

using UnityEngine;
using System;
using System.Collections.Generic;

[AddComponentMenu("Wwise/AkGameObj")]
///@brief This component represents a sound emitter in your scene. It will track its position and other game syncs such as Switches, RTPC and environment values.  You can add this to any object that will emit sound.  Note that if it is not present, Wwise will add it automatically, with the default values, to any Unity Game Object that is passed to Wwise.  
/// \sa
/// - \ref soundengine_gameobj
/// - \ref soundengine_events
/// - \ref soundengine_switch
/// - \ref soundengine_states
/// - \ref soundengine_environments
[ExecuteInEditMode] //ExecuteInEditMode necessary to maintain proper state of isStaticObject.
public class AkGameObj : MonoBehaviour 
{
	const int ALL_LISTENER_MASK = (1<<AkSoundEngine.AK_NUM_LISTENERS)-1;

	/// When not set to null, the emitter position will be offset relative to the Game Object position by the Position Offset
	public AkGameObjPositionOffsetData m_positionOffsetData = null;
	
	/// Is this object affected by Environment changes?  Set to false if not affected in order to save some useless calls.  Default is true.
    public bool isEnvironmentAware = true;
	private AkGameObjEnvironmentData m_envData = null;

	/// Listener 0 by default.
	public int listenerMask = 1; 

	/// Maintains and persists the Static setting of the gameobject, which is available only in the editor.
	[SerializeField]
	private bool isStaticObject = false;
	private AkGameObjPositionData m_posData = null;

    /// Cache the bounds to avoid calls to GetComponent()
    private Collider m_Collider;

    void Awake()
    {			
#if UNITY_EDITOR
        if (AkUtilities.IsMigrating)
        {
            return;
        }
#endif

		// If the object was marked as static, don't update its position to save cycles.
#if UNITY_EDITOR
		if (!UnityEditor.EditorApplication.isPlaying)	
		{
			UnityEditor.EditorApplication.update += this.CheckStaticStatus;
			return;
		}
#endif 
		if(!isStaticObject)
		{
			m_posData = new AkGameObjPositionData();
		}		
		
		// Cache the bounds to avoid calls to GetComponent()
		m_Collider = GetComponent<Collider>();
	
        //Register a Game Object in the sound engine, with its name.		
        AKRESULT res = AkSoundEngine.RegisterGameObj(gameObject, gameObject.name, (uint)(listenerMask & ALL_LISTENER_MASK));
        if (res == AKRESULT.AK_Success)
        {
            // Get position with offset
            Vector3 position = GetPosition();

            //Set the original position
            AkSoundEngine.SetObjectPosition(
                gameObject,
                position.x,
                position.y,
                position.z,
                transform.forward.x,
                transform.forward.y,
                transform.forward.z,
				transform.up.x,
				transform.up.y,
				transform.up.z);

            if (isEnvironmentAware)
            {
                m_envData = new AkGameObjEnvironmentData();
				//Check if this object is also an environment.
				m_envData.AddAkEnvironment(gameObject, gameObject);
            }
        }
    }
	
	private void CheckStaticStatus()
	{
#if UNITY_EDITOR

        if (AkUtilities.IsMigrating)
        {
            return;
        }

		if (gameObject != null && isStaticObject != gameObject.isStatic)
		{
			isStaticObject = gameObject.isStatic;
            UnityEditor.EditorUtility.SetDirty(this);
        }	
#endif
	}
	
	void OnEnable()
	{ 
#if UNITY_EDITOR
        if (AkUtilities.IsMigrating)
        {
            return;
        }
#endif

		//if enabled is set to false, then the update function wont be called
		enabled = !isStaticObject;
	}
	
    void OnDestroy()
    {
#if UNITY_EDITOR

        if (AkUtilities.IsMigrating)
        {
            return;
        }

		if (!UnityEditor.EditorApplication.isPlaying)	
		{
			UnityEditor.EditorApplication.update -= this.CheckStaticStatus;
		}
#endif
		// We can't do the code in OnDestroy if the gameObj is unregistered, so do it now.		
		AkUnityEventHandler[] eventHandlers = gameObject.GetComponents<AkUnityEventHandler>();
		foreach( AkUnityEventHandler handler in eventHandlers )
		{
			if( handler.triggerList.Contains(AkUnityEventHandler.DESTROY_TRIGGER_ID) )
			{
				handler.DoDestroy();
			}
		}

#if UNITY_EDITOR
		if (UnityEditor.EditorApplication.isPlaying)
#endif
        {
            if (AkSoundEngine.IsInitialized())
            {
                AkSoundEngine.UnregisterGameObj(gameObject);
            }
        }
    }

    void Update()
    {
#if UNITY_EDITOR

        if (AkUtilities.IsMigrating)
        {
            return;
        }

		if (!UnityEditor.EditorApplication.isPlaying)
		{
			return;
		}
#endif

		if (isEnvironmentAware && m_envData != null)
		{
			m_envData.UpdateAuxSend(gameObject, transform.position);
		}

		if (isStaticObject)
		{
			return;
		}

	    // Get position with offset
	    Vector3 position = GetPosition();

		//Didn't move.  Do nothing.
		if (m_posData.position == position && m_posData.forward == transform.forward && m_posData.up == transform.up)
			return;

		m_posData.position = position;
		m_posData.forward = transform.forward;
		m_posData.up = transform.up;

		//Update position
		AkSoundEngine.SetObjectPosition(
			gameObject,
			position.x,
			position.y,
			position.z,
			transform.forward.x,
			transform.forward.y,
			transform.forward.z,
			transform.up.x,
			transform.up.y,
			transform.up.z);
	}
	/// Gets the position including the position offset, if applyPositionOffset is enabled.
	/// \return  The position.
	public Vector3 GetPosition()
	{
		if (m_positionOffsetData != null)
		{
			// Get offset in world space
			Vector3 worldOffset = transform.rotation * m_positionOffsetData.positionOffset;
			
			// Add offset to gameobject position
			return transform.position + worldOffset;
		}

		return transform.position;
	}


    void OnTriggerEnter(Collider other)
    {
#if UNITY_EDITOR

        if (AkUtilities.IsMigrating)
        {
            return;
        }

		if (!UnityEditor.EditorApplication.isPlaying)
		{
			return;
		}
#endif

        if (isEnvironmentAware && m_envData != null)
        {
			m_envData.AddAkEnvironment(other.gameObject, gameObject);
        }
    }

    void OnTriggerExit(Collider other)
    {
#if UNITY_EDITOR

        if (AkUtilities.IsMigrating)
        {
            return;
        }

		if (!UnityEditor.EditorApplication.isPlaying)
		{
			return;
		}
#endif

        if (isEnvironmentAware && m_envData != null)
        {
			m_envData.RemoveAkEnvironment(other.gameObject, gameObject, m_Collider);
        }
    }

#if UNITY_EDITOR
	public void OnDrawGizmosSelected()
	{
		if (AkUtilities.IsMigrating)
		{
			return;
		}

		Vector3 position = GetPosition();
		Gizmos.DrawIcon(position, "WwiseAudioSpeaker.png", false);
	}
#endif

	#region WwiseMigration

#pragma warning disable 0414 // private field assigned but not used.

	[SerializeField]
	private AkGameObjPosOffsetData m_posOffsetData = null;

#pragma warning restore 0414 // private field assigned but not used.


#if UNITY_EDITOR

	public void Migrate9()
	{
		Debug.Log("WwiseUnity: AkGameObj.Migrate9");

		if ((listenerMask & ALL_LISTENER_MASK) == ALL_LISTENER_MASK)
		{
			listenerMask = 1;
		}
	}

	public void Migrate10()
	{
		Debug.Log("WwiseUnity: AkGameObj.Migrate10");

		if (m_posOffsetData != null)
		{
			m_positionOffsetData = new AkGameObjPositionOffsetData(true);
			m_positionOffsetData.positionOffset = m_posOffsetData.positionOffset;
			m_posOffsetData = null;
		}
	}

#endif

	#endregion
}
#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.