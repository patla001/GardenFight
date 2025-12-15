using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System.Collections;

/// <summary>
/// Simple single-player boss fight starter.
/// Replaces Host/Join buttons with a single "Play" button.
/// </summary>
public class BossFightStarter : MonoBehaviour
{
    [Header("UI References")]
    public Button playButton;
    public GameObject playButtonObject;
    public GameObject loadingText; // Optional: "Loading..." text
    
    [Header("Settings")]
    public float fadeDelay = 0.5f; // Delay before starting game
    
    private NetworkManager networkManager;

    void Start()
    {
        // Find the NetworkManager
        networkManager = FindFirstObjectByType<NetworkManager>();
        
        if (networkManager == null)
        {
            Debug.LogError("BossFightStarter: NetworkManager not found in scene!");
            return;
        }

        // Setup play button
        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayButtonClicked);
        }
        
        // Hide loading text initially
        if (loadingText != null)
        {
            loadingText.SetActive(false);
        }
    }

    public void OnPlayButtonClicked()
    {
        Debug.Log("Play button clicked - Starting boss fight!");
        StartCoroutine(StartBossFight());
    }

    IEnumerator StartBossFight()
    {
        // Hide play button
        if (playButtonObject != null)
        {
            playButtonObject.SetActive(false);
        }
        
        // Show loading text
        if (loadingText != null)
        {
            loadingText.SetActive(true);
        }

        // Small delay for visual feedback
        yield return new WaitForSeconds(fadeDelay);

        // Start as Host (this makes the game work as single-player with networking)
        networkManager.StartHost();
        
        Debug.Log("Boss fight started as Host!");
        
        // Hide loading text after a moment
        yield return new WaitForSeconds(0.5f);
        if (loadingText != null)
        {
            loadingText.SetActive(false);
        }
    }

    // Optional: Call this to restart the fight
    public void RestartBossFight()
    {
        networkManager.StopHost();
        
        // Show play button again
        if (playButtonObject != null)
        {
            playButtonObject.SetActive(true);
        }
    }
}
