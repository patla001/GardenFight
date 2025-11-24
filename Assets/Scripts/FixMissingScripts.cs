using UnityEngine;
using UnityEditor;
using System.Linq;

public class FixMissingScripts : EditorWindow
{
    [MenuItem("Tools/Fix Missing Scripts and Add NetworkIdentity")]
    static void ShowWindow()
    {
        GetWindow<FixMissingScripts>("Fix Prefabs");
    }

    void OnGUI()
    {
        GUILayout.Label("Fix Prefabs - Remove Missing Scripts & Add NetworkIdentity", EditorStyles.boldLabel);
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Fix RPG-Character Prefabs", GUILayout.Height(40)))
        {
            FixPrefabs();
        }
        
        GUILayout.Space(10);
        GUILayout.Label("This will:", EditorStyles.helpBox);
        GUILayout.Label("1. Remove all missing scripts from prefabs");
        GUILayout.Label("2. Add NetworkIdentity to root objects");
        GUILayout.Label("3. Enable Local Player Authority");
    }

    static void FixPrefabs()
    {
        string[] prefabPaths = new string[]
        {
            "Assets/RPG Character Animation Pack FREE/Prefabs/RPG-Character.prefab",
            "Assets/Prefabs/RPG-Character Variant.prefab"
        };

        int fixedCount = 0;

        foreach (string path in prefabPaths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogWarning($"Prefab not found: {path}");
                continue;
            }

            // Load prefab for editing
            string prefabPath = AssetDatabase.GetAssetPath(prefab);
            GameObject prefabInstance = PrefabUtility.LoadPrefabContents(prefabPath);

            if (prefabInstance == null)
            {
                Debug.LogError($"Could not load prefab: {path}");
                continue;
            }

            bool madeChanges = false;

            // Remove missing scripts from root and all children
            Component[] components = prefabInstance.GetComponentsInChildren<Component>(true);
            foreach (Component comp in components)
            {
                if (comp == null)
                {
                    // This is a missing script
                    Debug.Log($"Found missing script on {prefabInstance.name}");
                    GameObjectUtility.RemoveMonoBehavioursWithMissingScript(prefabInstance);
                    madeChanges = true;
                    break;
                }
            }

            // Add NetworkIdentity to root if missing
            Mirror.NetworkIdentity netId = prefabInstance.GetComponent<Mirror.NetworkIdentity>();
            if (netId == null)
            {
                netId = prefabInstance.AddComponent<Mirror.NetworkIdentity>();
                // Mirror v96+ handles authority automatically
                Debug.Log($"Added NetworkIdentity to {prefabInstance.name}");
                madeChanges = true;
            }
            else
            {
                Debug.Log($"NetworkIdentity already exists on {prefabInstance.name}");
            }

            // Save changes
            if (madeChanges)
            {
                PrefabUtility.SaveAsPrefabAsset(prefabInstance, prefabPath);
                fixedCount++;
                Debug.Log($"✅ Fixed and saved: {path}");
            }

            // Unload prefab
            PrefabUtility.UnloadPrefabContents(prefabInstance);
        }

        if (fixedCount > 0)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"<color=green>✅ Successfully fixed {fixedCount} prefab(s)!</color>");
            EditorUtility.DisplayDialog("Success!", $"Fixed {fixedCount} prefab(s)!\n\nNetworkIdentity added with Local Player Authority enabled.", "OK");
        }
        else
        {
            Debug.Log("No changes needed.");
            EditorUtility.DisplayDialog("Info", "No changes were needed. Prefabs are already correct!", "OK");
        }
    }
}

