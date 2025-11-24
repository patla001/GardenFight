using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

public class CleanupMissingScripts
{
    [MenuItem("Tools/Cleanup Missing Scripts in Scene")]
    static void CleanupScene()
    {
        int fixedCount = 0;
        
        // Get all GameObjects in the current scene
        GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        
        foreach (GameObject go in allObjects)
        {
            int missingCount = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);
            
            if (missingCount > 0)
            {
                Debug.Log($"Found {missingCount} missing script(s) on: {go.name}");
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
                fixedCount++;
                Debug.Log($"✅ Cleaned up: {go.name}");
            }
        }
        
        if (fixedCount > 0)
        {
            // Mark scene as dirty to save changes
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            
            Debug.Log($"<color=green>✅ Cleaned up {fixedCount} GameObject(s) with missing scripts!</color>");
            EditorUtility.DisplayDialog(
                "Cleanup Complete!", 
                $"Removed missing scripts from {fixedCount} GameObject(s)!\n\nPlease save the scene (Ctrl/Cmd + S).", 
                "OK"
            );
        }
        else
        {
            Debug.Log("No missing scripts found in scene.");
            EditorUtility.DisplayDialog("Info", "No missing scripts found in the current scene.", "OK");
        }
    }
}

