using UnityEngine;
using UnityEditor;
using Mirror;

public class QuickAddTransport
{
    [MenuItem("Tools/Quick Fix - Add Transport to NetworkManager")]
    static void AddTransportToNetworkManager()
    {
        // Find NetworkManager in scene
        MyNetManager netManager = Object.FindFirstObjectByType<MyNetManager>();
        
        if (netManager == null)
        {
            EditorUtility.DisplayDialog("Error", "No NetworkManager found in scene!\n\nPlease open FightScene.unity first.", "OK");
            return;
        }

        GameObject netManagerObj = netManager.gameObject;

        // Check if transport already exists
        Transport existingTransport = netManagerObj.GetComponent<Transport>();
        if (existingTransport != null)
        {
            Debug.Log($"✅ Transport already exists: {existingTransport.GetType().Name}");
            EditorUtility.DisplayDialog("Already Setup", $"NetworkManager already has a transport:\n{existingTransport.GetType().Name}", "OK");
            return;
        }

        // Try to add KcpTransport
        bool added = false;
        
        // Method 1: Try direct namespace
        try
        {
            var kcpTransport = netManagerObj.AddComponent<kcp2k.KcpTransport>();
            if (kcpTransport != null)
            {
                Debug.Log("✅ Added KcpTransport to NetworkManager");
                added = true;
            }
        }
        catch
        {
            Debug.LogWarning("Could not add KcpTransport directly, trying alternative...");
        }

        // Method 2: Try TelepathyTransport as fallback
        if (!added)
        {
            try
            {
                var telepathyTransport = netManagerObj.AddComponent<Mirror.TelepathyTransport>();
                if (telepathyTransport != null)
                {
                    Debug.Log("✅ Added TelepathyTransport to NetworkManager");
                    added = true;
                }
            }
            catch
            {
                Debug.LogError("Could not add TelepathyTransport either!");
            }
        }

        if (added)
        {
            // Mark scene as dirty so Unity knows to save changes
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(netManagerObj.scene);
            
            EditorUtility.DisplayDialog("Success!", "✅ Transport has been added to NetworkManager!\n\nPlease save the scene (Ctrl/Cmd + S) and try playing again.", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Error", "❌ Could not add any transport component.\n\nPlease add 'Kcp Transport' manually through the Inspector.", "OK");
        }
    }
}

