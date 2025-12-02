using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Vector3 offset = new Vector3(0, 8, -10);
    private Transform player;
    public int rotationSpeed = 5;
    public bool enableRotation = false; // Set to true if you want camera rotation

    // Start is called before the first frame update
    void Start()
    {
        // Set camera to a safe starting position (above and behind origin)
        if (transform.position.y < 1f)
        {
            transform.position = new Vector3(0, 10, -15);
            transform.rotation = Quaternion.Euler(30, 0, 0);
        }
        
        // Try to find player by parent first (old method)
        player = transform.parent;
        
        if (player != null)
        {
            // Old method: camera was child of player
            transform.parent = null;
            offset = transform.position - player.position;
        }
        else
        {
            // New method: find the local player in scene
            StartCoroutine(FindLocalPlayer());
        }
    }

    IEnumerator FindLocalPlayer()
    {
        // Keep trying to find player for up to 5 seconds
        float timeout = 5f;
        float elapsed = 0f;
        
        while (elapsed < timeout)
        {
            GameObject localPlayer = GameObject.Find("local player");
            if (localPlayer != null)
            {
                player = localPlayer.transform;
                Debug.Log("Camera found local player!");
                yield break; // Exit coroutine successfully
            }
            
            elapsed += 0.2f;
            yield return new WaitForSeconds(0.2f);
        }
        
        Debug.LogWarning("Camera could not find 'local player' after 5 seconds");
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null) return; // Don't do anything if no player found
        
        // Follow player with offset
        transform.position = player.position + offset;
        
        // Optional rotation around player
        if (enableRotation)
        {
            transform.RotateAround(player.position, player.transform.up, rotationSpeed * Time.deltaTime);
        }
        else
        {
            // Look at player
            transform.LookAt(player.position + Vector3.up * 2); // Look at player's upper body
        }
    }
}
