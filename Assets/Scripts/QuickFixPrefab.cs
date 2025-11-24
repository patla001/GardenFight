using UnityEngine;
using UnityEditor;

public class QuickFixPrefab
{
    [MenuItem("Assets/Add NetworkIdentity to Prefab", false, 20)]
    static void AddNetworkIdentity()
    {
        // Get selected prefab
        GameObject selectedPrefab = Selection.activeObject as GameObject;
        
        if (selectedPrefab == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select a prefab in the Project window first!", "OK");
            return;
        }

        string path = AssetDatabase.GetAssetPath(selectedPrefab);
        if (string.IsNullOrEmpty(path) || !path.EndsWith(".prefab"))
        {
            EditorUtility.DisplayDialog("Error", "Please select a valid prefab file!", "OK");
            return;
        }

        Debug.Log($"Processing prefab: {path}");

        // Load prefab contents
        GameObject prefabInstance = PrefabUtility.LoadPrefabContents(path);

        if (prefabInstance == null)
        {
            EditorUtility.DisplayDialog("Error", $"Could not load prefab: {path}", "OK");
            return;
        }

        bool madeChanges = false;

        // Remove missing scripts
        int removedCount = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(prefabInstance);
        if (removedCount > 0)
        {
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(prefabInstance);
            Debug.Log($"Removed {removedCount} missing script(s) from {prefabInstance.name}");
            madeChanges = true;
        }

        // Add NetworkIdentity if missing
        Mirror.NetworkIdentity netId = prefabInstance.GetComponent<Mirror.NetworkIdentity>();
        if (netId == null)
        {
            prefabInstance.AddComponent<Mirror.NetworkIdentity>();
            Debug.Log($"✅ Added NetworkIdentity to {prefabInstance.name}");
            madeChanges = true;
        }
        else
        {
            Debug.Log($"NetworkIdentity already exists on {prefabInstance.name}");
        }

        // Save if changes were made
        if (madeChanges)
        {
            PrefabUtility.SaveAsPrefabAsset(prefabInstance, path);
            Debug.Log($"<color=green>✅ Saved: {path}</color>");
        }

        // Cleanup
        PrefabUtility.UnloadPrefabContents(prefabInstance);

        // Refresh
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (madeChanges)
        {
            EditorUtility.DisplayDialog("Success!", $"Fixed prefab: {selectedPrefab.name}\n\n✅ NetworkIdentity added!", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Info", "Prefab is already correct - no changes needed!", "OK");
        }
    }
}

