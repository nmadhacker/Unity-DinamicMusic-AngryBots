#if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.
//////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2012 Audiokinetic Inc. / All Rights Reserved
//
//////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Runtime.InteropServices;

public class AkAuxSendArray
{
	const int MAX_COUNT = AkEnvironment.MAX_NB_ENVIRONMENTS;
	const int SIZE_OF_AKAUXSENDVALUE = sizeof(uint) + sizeof(float);

	public AkAuxSendArray()
	{
		m_Buffer = Marshal.AllocHGlobal(MAX_COUNT * SIZE_OF_AKAUXSENDVALUE);
		m_Count = 0;
	}

	~AkAuxSendArray()
	{
		Marshal.FreeHGlobal(m_Buffer);
		m_Buffer = IntPtr.Zero;
	}

	IntPtr GetObjectPtr(uint index)
	{
		return (IntPtr)(m_Buffer.ToInt64() + SIZE_OF_AKAUXSENDVALUE * index);
	}

	public void Reset()
	{
		m_Count = 0;
	}

	public void Add(uint in_EnvID, float in_fValue)
	{
		if (isFull)
			return;

		AkSoundEnginePINVOKE.CSharp_AkAuxSendValueProxy_set(GetObjectPtr(m_Count), in_EnvID, in_fValue);
		m_Count++;
    }

    public bool Contains(uint in_EnvID)
	{
		if (m_Buffer == IntPtr.Zero)
			return false;

		for (uint i = 0; i < m_Count; i++)
			if (in_EnvID == AkSoundEnginePINVOKE.CSharp_AkAuxSendValue_auxBusID_get(GetObjectPtr(i)))
				return true;

		return false;
	}

	public bool isFull
	{
		get { return m_Count >= MAX_COUNT || m_Buffer == IntPtr.Zero; }
	}

	public IntPtr m_Buffer;
	public uint m_Count;
};
#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.