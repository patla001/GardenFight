using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using Mirror;

[InitializeOnLoad]
public class SetupNetworkManager
{
    static SetupNetworkManager()
    {
        EditorApplication.delayCall += CheckAndSetupNetworkManager;
    }

    static void CheckAndSetupNetworkManager()
    {
        // Only run in play mode
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            // Check if NetworkManager exists
            MyNetManager netManager = Object.FindFirstObjectByType<MyNetManager>();
            
            if (netManager == null)
            {
                Debug.LogWarning("⚠️ No NetworkManager found in scene! Creating one...");
                CreateNetworkManager();
            }
            else
            {
                // Check if transport is assigned
                if (netManager.transport == null)
                {
                    Debug.LogWarning("⚠️ NetworkManager missing Transport! Adding one...");
                    AddTransportToNetworkManager(netManager.gameObject);
                }
            }
        }
    }

    [MenuItem("Tools/Setup NetworkManager in Scene")]
    static void ManualSetup()
    {
        MyNetManager existingManager = Object.FindFirstObjectByType<MyNetManager>();
        
        if (existingManager != null)
        {
            Debug.Log("NetworkManager already exists. Checking configuration...");
            
            if (existingManager.transport == null)
            {
                AddTransportToNetworkManager(existingManager.gameObject);
            }
            
            CheckPlayerPrefab(existingManager);
            CheckDiscovery(existingManager);
            
            EditorUtility.DisplayDialog("Setup Complete", "NetworkManager is properly configured!", "OK");
            return;
        }

        CreateNetworkManager();
        EditorUtility.DisplayDialog("Setup Complete", "NetworkManager has been created and configured!", "OK");
    }

    static void CreateNetworkManager()
    {
        GameObject netManagerObj = new GameObject("NetworkManager");
        MyNetManager netManager = netManagerObj.AddComponent<MyNetManager>();
        
        AddTransportToNetworkManager(netManagerObj);
        CheckPlayerPrefab(netManager);
        CheckDiscovery(netManager);
        
        Debug.Log("<color=green>✅ NetworkManager created successfully!</color>");
    }

    static void AddTransportToNetworkManager(GameObject netManagerObj)
    {
        // Check if any transport exists
        Transport existingTransport = netManagerObj.GetComponent<Transport>();
        
        if (existingTransport == null)
        {
            // Add KcpTransport (default Mirror transport) - it's in kcp2k namespace
            System.Type kcpType = System.Type.GetType("kcp2k.KcpTransport, Mirror");
            if (kcpType != null)
            {
                netManagerObj.AddComponent(kcpType);
                Debug.Log("✅ Added KcpTransport to NetworkManager");
            }
            else
            {
                // Fallback to TelepathyTransport
                System.Type telepathyType = System.Type.GetType("Mirror.TelepathyTransport, Mirror");
                if (telepathyType != null)
                {
                    netManagerObj.AddComponent(telepathyType);
                    Debug.Log("✅ Added TelepathyTransport to NetworkManager");
                }
                else
                {
                    Debug.LogError("❌ Could not find any transport type!");
                }
            }
        }
        else
        {
            Debug.Log($"✅ Transport already exists: {existingTransport.GetType().Name}");
        }
    }

    static void CheckPlayerPrefab(MyNetManager netManager)
    {
        if (netManager.playerPrefab == null)
        {
            // Try to find the player prefab
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/RPG-Character Variant.prefab");
            
            if (playerPrefab != null)
            {
                netManager.playerPrefab = playerPrefab;
                Debug.Log("✅ Assigned Player Prefab: RPG-Character Variant");
            }
            else
            {
                Debug.LogWarning("⚠️ Could not find player prefab at: Assets/Prefabs/RPG-Character Variant.prefab");
            }
        }
    }

    static void CheckDiscovery(MyNetManager netManager)
    {
        SimpleNetworkDiscovery discovery = netManager.GetComponent<SimpleNetworkDiscovery>();
        
        if (discovery == null)
        {
            discovery = netManager.gameObject.AddComponent<SimpleNetworkDiscovery>();
            Debug.Log("✅ Added SimpleNetworkDiscovery component");
        }
    }
}

