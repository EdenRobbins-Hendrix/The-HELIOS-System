using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class SceneTester
{
    public static void TestSceneLoad()
    {
        string scenePath = "Assets/Scenes/SampleScene.unity";

        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        if (scene.IsValid())
        {
            Debug.Log("Good! Scene loaded successfully: " + scenePath);
        }
        else
        {
            Debug.LogError("Bad! Scene failed to load: " + scenePath);
        }

        EditorApplication.Exit(0);
    }
}
