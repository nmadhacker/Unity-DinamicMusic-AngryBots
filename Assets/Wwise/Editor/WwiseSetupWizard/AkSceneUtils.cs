#if UNITY_5_0 || UNITY_5_1 || UNITY_5_2
#define UNITY_5_0_TO_5_2
#endif

#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System;

public class AkSceneUtils
{
#if !UNITY_5_0_TO_5_2
    private static UnityEngine.SceneManagement.Scene m_currentScene;
#endif

    public static void CreateNewScene()
    {
#if UNITY_5_0_TO_5_2
        EditorApplication.NewScene();
#else
        m_currentScene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.DefaultGameObjects);
#endif
    }

    public static void OpenExistingScene(string scene)
    {
        if (string.IsNullOrEmpty(scene))
        {
            return;
        }

#if UNITY_5_0_TO_5_2
        EditorApplication.OpenScene(scene);
#else
        m_currentScene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scene);
#endif
    }

    public static string GetCurrentScene()
    {
#if UNITY_5_0_TO_5_2
        return EditorApplication.currentScene;
#else
        UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        return scene.path;
#endif
    }

    public static void SaveCurrentScene(string scene)
    {
#if UNITY_5_0_TO_5_2

        if (scene == null)
        {
            EditorApplication.SaveScene();
        }
        else
        {
            EditorApplication.SaveScene(scene);
        }
#else

        bool result;
        
        if (scene == null)
        {
            result = !UnityEditor.SceneManagement.EditorSceneManager.SaveScene(m_currentScene);
        }
        else
        {
            result = !UnityEditor.SceneManagement.EditorSceneManager.SaveScene(m_currentScene, scene);
        }

        if (result)
        {
            throw new Exception("Error occured while saving migrated scenes.");
        }
#endif
    }
}

#endif