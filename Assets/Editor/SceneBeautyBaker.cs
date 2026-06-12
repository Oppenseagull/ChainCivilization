using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneBeautyBaker
{
    const string MenuPath = "Chain Civilization/Bake Scene Beauty To Current Scene";

    [MenuItem(MenuPath)]
    public static void BakeCurrentScene()
    {
        if (Application.isPlaying)
        {
            EditorUtility.DisplayDialog(
                "Bake Scene Beauty",
                "Exit Play Mode before baking. Baking writes objects into the opened scene.",
                "OK");
            return;
        }

        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid())
        {
            EditorUtility.DisplayDialog("Bake Scene Beauty", "No active scene is open.", "OK");
            return;
        }

        GameObject tool = new GameObject("SceneBeautyDirector_BakeTool")
        {
            hideFlags = HideFlags.HideAndDontSave
        };

        try
        {
            SceneBeautyDirector director = tool.AddComponent<SceneBeautyDirector>();
            director.Apply();

            EditorSceneManager.MarkSceneDirty(scene);
            bool saved = EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog(
                "Bake Scene Beauty",
                saved
                    ? "Scene beauty was baked into the current scene and saved."
                    : "Scene beauty was baked, but Unity did not save the scene.",
                "OK");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(tool);
        }
    }

    public static void BakeCurrentSceneFromCommandLine()
    {
        if (Application.isPlaying)
        {
            return;
        }

        GameObject tool = new GameObject("SceneBeautyDirector_BakeTool")
        {
            hideFlags = HideFlags.HideAndDontSave
        };

        try
        {
            SceneBeautyDirector director = tool.AddComponent<SceneBeautyDirector>();
            director.Apply();

            Scene scene = SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(tool);
        }
    }
}
