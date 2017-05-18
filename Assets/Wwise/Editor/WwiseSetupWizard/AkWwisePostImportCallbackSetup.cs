#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

[InitializeOnLoad]
public class AkWwisePostImportCallbackSetup
{
    static AkWwisePostImportCallbackSetup()
    {
        string[] arguments = Environment.GetCommandLineArgs();
        if (Array.IndexOf(arguments, "-nographics") != -1 && Array.IndexOf(arguments, "-wwiseEnableWithNoGraphics") == -1)
        {
            return;
        }

        EditorApplication.delayCall += CheckMigrationStatus;
    }

    private static void CheckMigrationStatus()
    {
        try
        {
            int migrationStart;
            int migrationStop;

            if (IsMigrationPending(out migrationStart, out migrationStop))
            {
                m_scheduledMigrationStart = migrationStart;
                m_scheduledMigrationStop = migrationStop;
                ScheduleMigration();
            }
            else
            {
                RefreshCallback();
            }
        }
        catch (Exception e)
        {
            Debug.LogError("WwiseUnity: Error during migration: " + e);
        }
    }

    private static int m_scheduledMigrationStart;
    private static int m_scheduledMigrationStop;

    private static void ScheduleMigration()
    {
        // TODO: Is delayCall wiped out during a script reload?
        // If not, guard against having a delayCall from a previously loaded code being run after the new loading.

        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
        {
            // Skip if not in the right mode, wait for the next callback to see if we can proceed then.
            EditorApplication.delayCall += ScheduleMigration;
            return;
        }

        try
        {
            WwiseSetupWizard.PerformMigration(m_scheduledMigrationStart, m_scheduledMigrationStop);
            EditorApplication.delayCall += RefreshCallback;
        }
        catch (Exception e)
        {
            Debug.LogError("WwiseUnity: Error during migration: " + e);
        }
    }

    private static bool IsMigrationPending(out int migrationStart, out int migrationStop)
    {
        migrationStart = 0;
        migrationStop = 0;

        string filename = Application.dataPath + "/../.WwiseLauncherLockFile";

        if (!File.Exists(filename))
        {
            return false;
        }

        string fileContent = File.ReadAllText(filename);

        // Instantiate the regular expression object.
        Regex r = new Regex("{\"migrateStart\":(\\d+),\"migrateStop\":(\\d+),.*}", RegexOptions.IgnoreCase);

        // Match the regular expression pattern against a text string.
        Match m = r.Match(fileContent);

        if (!m.Success ||
            m.Groups.Count < 2 ||
            m.Groups[1].Captures.Count < 1 ||
            m.Groups[2].Captures.Count < 1 ||
            !int.TryParse(m.Groups[1].Captures[0].ToString(), out migrationStart) ||
            !int.TryParse(m.Groups[2].Captures[0].ToString(), out migrationStop))
        {
            throw new Exception("Error in the file format of .WwiseLauncherLockFile.");
        }

        return true;
    }

	private static void RefreshCallback()
	{
		PostImportFunction();
        if (File.Exists(Path.Combine(Application.dataPath, WwiseSettings.WwiseSettingsFilename)))
        {
			AkPluginActivator.Update();
			AkPluginActivator.RefreshPlugins();

#if UNITY_5
       		// Check if platform is supported and installed. PluginImporter might contain
			// erroneous data when application is compiling or updating, so skip this if
			// that is the case.
            if (!EditorApplication.isCompiling && !EditorApplication.isUpdating)
            {
				string Msg;
				if (!CheckPlatform(out Msg))
				{
					EditorUtility.DisplayDialog("Warning", Msg, "OK");
				}
			}
#endif
        }
    }

    private static void PostImportFunction()
    {
        EditorApplication.hierarchyWindowChanged += CheckWwiseGlobalExistance;
        EditorApplication.delayCall += CheckPicker;

        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
        {
            return;
        }

        try
		{
			if (File.Exists(Application.dataPath + Path.DirectorySeparatorChar + WwiseSettings.WwiseSettingsFilename))
			{
				WwiseSetupWizard.Settings = WwiseSettings.LoadSettings();
				AkWwiseProjectInfo.GetData();
			}

			if (!string.IsNullOrEmpty(WwiseSetupWizard.Settings.WwiseProjectPath))
			{
				AkWwisePicker.PopulateTreeview();
				if (AkWwiseProjectInfo.GetData().autoPopulateEnabled)
				{
                    AkWwiseWWUBuilder.StartWWUWatcher();
				}
			}
		}
		catch (Exception e)
		{
			Debug.Log(e.ToString());
		}
		
		//Check if a WwiseGlobal object exists in the current scene	
		CheckWwiseGlobalExistance();
	}

    private static bool CheckPlatform(out string Msg)
    {
        Msg = string.Empty;
#if UNITY_WSA_8_0
        Msg = "The Wwise Unity integration does not support the Windows Store 8.0 SDK.";
        return false;
#else
        // Start by checking if the integration supports the platform
        switch (EditorUserBuildSettings.activeBuildTarget)
        {
            case BuildTarget.PSM:
            case BuildTarget.SamsungTV:
#if UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3
            case BuildTarget.BlackBerry:
            case BuildTarget.StandaloneGLESEmu:
            case BuildTarget.WebPlayer:
            case BuildTarget.WebPlayerStreamed:
#endif
            case BuildTarget.Tizen:
            case BuildTarget.WebGL:
                Msg = "The Wwise Unity integration does not support this platform.";
                return false;
        }

        // Then check if the integration is installed for this platform
        PluginImporter[] importers = PluginImporter.GetImporters(EditorUserBuildSettings.activeBuildTarget);
        bool found = false;
        foreach (PluginImporter imp in importers)
        {
            if(imp.assetPath.Contains("AkSoundEngine"))
            {
                found = true;
                break;
            }
        }

        if(!found)
        {
            Msg = "The Wwise Unity integration for the " + EditorUserBuildSettings.activeBuildTarget.ToString() + " platform is currently not installed.";
            return false;
        }

        return true;
#endif
    }

    private static void RefreshPlugins()
	{
		if (string.IsNullOrEmpty(AkWwiseProjectInfo.GetData().CurrentPluginConfig))
		{
			AkWwiseProjectInfo.GetData().CurrentPluginConfig = AkPluginActivator.CONFIG_PROFILE;
		}

		AkPluginActivator.RefreshPlugins();
	}
	
    private static void ClearConsole()
    {
         var logEntries = System.Type.GetType("UnityEditorInternal.LogEntries,UnityEditor.dll");
         var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
         clearMethod.Invoke(null,null);
    }
	
	public static void CheckPicker()
	{
		if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
		{
			// Skip if not in the right mode, wait for the next callback to see if we can proceed then.
			EditorApplication.delayCall += CheckPicker;
            return;
		}

		WwiseSettings settings = WwiseSettings.LoadSettings();

		if (!settings.CreatedPicker)
		{
            // Delete all the ghost tabs (Failed to load).
            EditorWindow[] windows = Resources.FindObjectsOfTypeAll<EditorWindow>();
            if (windows != null && windows.Length > 0)
            {
                foreach (EditorWindow window in windows)
                {
                    if (window.titleContent.text.Equals("Failed to load") || 
                        window.titleContent.text.Equals("AkWwisePicker"))
                    {
                        try
                        {
                            window.Close();
                        }
                        catch (Exception)
                        {
                            // Do nothing here, this shoudn't cause any problem, however there has been
                            // occurences of Unity crashing on a null reference inside that method.
                        }
                    }
                }
            }

            ClearConsole();

            // TODO: If no scene is loaded and we are using the demo scene, automatically load it to display it.

			// Populate the picker
			AkWwiseProjectInfo.GetData(); // Load data
			if (!String.IsNullOrEmpty(settings.WwiseProjectPath))
			{
				AkWwiseProjectInfo.Populate();
				AkWwisePicker.init();
					
				if (AkWwiseProjectInfo.GetData().autoPopulateEnabled)
				{
					AkWwiseWWUBuilder.StartWWUWatcher();
				}
					
				settings.CreatedPicker = true;
				WwiseSettings.SaveSettings(settings);
			}
		}

        EditorApplication.delayCall += CheckPendingExecuteMethod;
	}

    // TODO: Put this in AkUtilities?
    private static void ExecuteMethod(string method)
    {
        string className = null;
        string methodName = null;
        
        Regex r = new Regex("(.+)\\.(.+)", RegexOptions.IgnoreCase);

        Match m = r.Match(method);

        if (!m.Success ||
            m.Groups.Count < 3 ||
            m.Groups[1].Captures.Count < 1 ||
            m.Groups[2].Captures.Count < 1)
        {
            Debug.LogError("WwiseUnity: Error parsing wwiseExecuteMethod parameter: " + method);
            return;
        }
        
        className = m.Groups[1].Captures[0].ToString();
        methodName = m.Groups[2].Captures[0].ToString();

        try
        {
            Type type = System.Type.GetType(className);
            MethodInfo clearMethod = type.GetMethod(methodName, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            clearMethod.Invoke(null, null);
        }
        catch (Exception e)
        {
            Debug.LogError("WwiseUnity: Exception caught when calling " + method + ": " + e.ToString());
        }
    }

    private static bool m_pendingExecuteMethodCalled = false;

    private static void CheckPendingExecuteMethod()
    {
        string[] arguments = Environment.GetCommandLineArgs();
        int indexOfCommand = Array.IndexOf(arguments, "-wwiseExecuteMethod");

        if (!m_pendingExecuteMethodCalled && indexOfCommand != -1 && arguments.Length > (indexOfCommand + 1))
        {
            string methodToExecute = arguments[indexOfCommand + 1];

            ExecuteMethod(methodToExecute);
            m_pendingExecuteMethodCalled = true;
        }
    }
	
	private static string s_CurrentScene = null;
	public static void CheckWwiseGlobalExistance()
	{
        WwiseSettings settings = WwiseSettings.LoadSettings();
#if UNITY_5_0 || UNITY_5_1 || UNITY_5_2
        if (String.IsNullOrEmpty(EditorApplication.currentScene) || s_CurrentScene != EditorApplication.currentScene)
#else
        string activeSceneName = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().name;
        if (String.IsNullOrEmpty(s_CurrentScene) || !s_CurrentScene.Equals(activeSceneName))
#endif
		{
			// Look for a game object which has the initializer component
			AkInitializer[] AkInitializers = UnityEngine.Object.FindObjectsOfType(typeof(AkInitializer)) as AkInitializer[];
			if (AkInitializers.Length == 0)
			{
                if (settings.CreateWwiseGlobal == true)
                {
                    //No Wwise object in this scene, create one so that the sound engine is initialized and terminated properly even if the scenes are loaded
                    //in the wrong order.
                    GameObject objWwise = new GameObject("WwiseGlobal");

                    //Attach initializer and terminator components
                    AkInitializer init = objWwise.AddComponent<AkInitializer>();
                    AkWwiseProjectInfo.GetData().CopyInitSettings(init);
                }
			}
			else
			{
                if (settings.CreateWwiseGlobal == false && AkInitializers[0].gameObject.name == "WwiseGlobal")
                {
                    GameObject.DestroyImmediate(AkInitializers[0].gameObject);
                }
				//All scenes will share the same initializer.  So expose the init settings consistently across scenes.
				AkWwiseProjectInfo.GetData().CopyInitSettings(AkInitializers[0]);
			}

			AkAudioListener[] akAudioListeners = UnityEngine.Object.FindObjectsOfType(typeof(AkAudioListener)) as AkAudioListener[];
            if (akAudioListeners.Length == 0)
            {
                // Remove the audio listener script
                if (Camera.main != null && settings.CreateWwiseListener == true)
                {
                    AudioListener listener = Camera.main.gameObject.GetComponent<AudioListener>();
                    if (listener != null)
                    {
                        Component.DestroyImmediate(listener);
                    }

                    // Add the AkAudioListener script
                    if (Camera.main.gameObject.GetComponent<AkAudioListener>() == null)
                    {
                        Camera.main.gameObject.AddComponent<AkAudioListener>();
                    }
                }
            }
            else
            {
                foreach (AkAudioListener akListener in akAudioListeners)
                {
                    if (settings.CreateWwiseListener == false && akListener.gameObject == Camera.main.gameObject)
                    {
                        Component.DestroyImmediate(akListener);
                    }
                }
            }


#if UNITY_5_0 || UNITY_5_1 || UNITY_5_2
			s_CurrentScene = EditorApplication.currentScene;
#else
			s_CurrentScene = activeSceneName;
#endif
		}
	}
}

#endif // UNITY_EDITOR