using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class AutoFixPrefabs
{
    static AutoFixPrefabs()
    {
        // Run once when Unity loads/compiles
        EditorApplication.delayCall += RunFix;
    }

    [MenuItem("Tools/Fix Player Prefabs Now")]
    static void ManualFix()
    {
        EditorPrefs.DeleteKey("AutoFixPrefabs_HasRun_v2");
        RunFix();
    }

    static void RunFix()
    {
        // Check if we've already run this fix
        string prefKey = "AutoFixPrefabs_HasRun_v2";
        if (EditorPrefs.GetBool(prefKey, false))
        {
            return; // Already fixed
        }

        Debug.Log("<color=cyan>🔧 Auto-fixing player prefabs...</color>");

        string[] prefabPaths = new string[]
        {
            "Assets/RPG Character Animation Pack FREE/Prefabs/RPG-Character.prefab",
            "Assets/Prefabs/RPG-Character Variant.prefab",
            "Assets/Prefabs/Magic fire 1.prefab"
        };

        int fixedCount = 0;

        foreach (string path in prefabPaths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogWarning($"⚠️ Prefab not found: {path}");
                continue;
            }

            try
            {
                // Load prefab contents
                GameObject prefabInstance = PrefabUtility.LoadPrefabContents(path);

                if (prefabInstance == null)
                {
                    Debug.LogError($"❌ Could not load prefab: {path}");
                    continue;
                }

                bool madeChanges = false;

                // Remove missing scripts
                int missingScripts = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(prefabInstance);
                if (missingScripts > 0)
                {
                    GameObjectUtility.RemoveMonoBehavioursWithMissingScript(prefabInstance);
                    Debug.Log($"  🧹 Removed {missingScripts} missing script(s) from {prefabInstance.name}");
                    madeChanges = true;
                }

                // Add NetworkIdentity if missing
                Mirror.NetworkIdentity netId = prefabInstance.GetComponent<Mirror.NetworkIdentity>();
                if (netId == null)
                {
                    prefabInstance.AddComponent<Mirror.NetworkIdentity>();
                    Debug.Log($"  ✅ Added NetworkIdentity to {prefabInstance.name}");
                    madeChanges = true;
                }

                // Save if changes were made
                if (madeChanges)
                {
                    PrefabUtility.SaveAsPrefabAsset(prefabInstance, path);
                    Debug.Log($"  💾 Saved: {path}");
                    fixedCount++;
                }

                // Cleanup
                PrefabUtility.UnloadPrefabContents(prefabInstance);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ Error processing {path}: {ex.Message}");
            }
        }

        if (fixedCount > 0)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"<color=green>🎉 Successfully auto-fixed {fixedCount} prefab(s)!</color>");
            Debug.Log("<color=green>✅ NetworkIdentity errors should now be resolved!</color>");
            
            // Mark as complete so we don't run again
            EditorPrefs.SetBool(prefKey, true);
            
            EditorUtility.DisplayDialog(
                "Auto-Fix Complete!", 
                $"Successfully fixed {fixedCount} prefab(s)!\n\n✅ NetworkIdentity added to player prefabs.\n\nThe red Mirror errors should now be gone!", 
                "Awesome!"
            );
        }
        else
        {
            Debug.Log("ℹ️ Prefabs are already correct - no auto-fix needed.");
            EditorPrefs.SetBool(prefKey, true);
        }
    }

    [MenuItem("Tools/Reset Auto-Fix (Run Again)")]
    static void ResetAutoFix()
    {
        EditorPrefs.DeleteKey("AutoFixPrefabs_HasRun_v2");
        Debug.Log("✅ Auto-fix reset. It will run again on next compile.");
        EditorUtility.DisplayDialog("Reset Complete", "Auto-fix will run again when Unity recompiles scripts.", "OK");
    }
}

